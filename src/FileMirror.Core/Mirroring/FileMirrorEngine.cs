using System;
using System.Collections.Generic;
using System.IO;
using FileMirror.Core.Config;
using FileMirror.Core.Monitoring;
using FileMirror.Core.Storage;

namespace FileMirror.Core.Mirroring;

public class FileMirrorEngine
{
    private readonly ChangeQueue _changeQueue = new();

    public void ProcessChange(SourceMapping mapping, FileSystemEvent change)
    {
        string sourcePath = change.Path;
        string targetPath = mapping.TargetPath + sourcePath[mapping.SourcePath.Length..];

        switch (change.Type)
        {
            case FileSystemEventType.Created:
                HandleCreated(sourcePath, targetPath);
                break;
            case FileSystemEventType.Changed:
                HandleChanged(sourcePath, targetPath);
                break;
            case FileSystemEventType.Deleted:
                HandleDeleted(sourcePath, targetPath);
                break;
            case FileSystemEventType.Renamed:
                HandleRenamed(sourcePath, targetPath, change.OldPath);
                break;
        }
    }

    private void HandleCreated(string sourcePath, string targetPath)
    {
        if (Directory.Exists(sourcePath))
        {
            Directory.CreateDirectory(targetPath);
            Directory.SetLastWriteTime(targetPath, File.GetLastWriteTime(sourcePath));
        }
        else if (File.Exists(sourcePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            File.Copy(sourcePath, targetPath, true);
            File.SetAttributes(targetPath, File.GetAttributes(sourcePath));
        }
    }

    private void HandleChanged(string sourcePath, string targetPath)
    {
        if (File.Exists(sourcePath))
        {
            File.Copy(sourcePath, targetPath, true);
            File.SetAttributes(targetPath, File.GetAttributes(sourcePath));
        }
    }

    private void HandleDeleted(string sourcePath, string targetPath)
    {
        if (File.Exists(targetPath))
        {
            File.Delete(targetPath);
        }
        else if (Directory.Exists(targetPath))
        {
            Directory.Delete(targetPath, true);
        }
    }

    private void HandleRenamed(string sourcePath, string targetPath, string? oldPath)
    {
        if (oldPath == null)
        {
            return;
        }

        string oldTargetPath = targetPath[..^Path.GetFileName(targetPath).Length] +
                                Path.GetFileName(oldPath);

        if (File.Exists(oldPath))
        {
            if (File.Exists(oldTargetPath))
            {
                File.Delete(oldTargetPath);
            }
            HandleCreated(sourcePath, targetPath);
        }
        else if (Directory.Exists(oldPath))
        {
            if (Directory.Exists(oldTargetPath))
            {
                Directory.Delete(oldTargetPath, true);
            }
            HandleCreated(sourcePath, targetPath);
        }
    }

    public void QueueForOffline(FileSystemEvent change)
    {
        _changeQueue.Enqueue(change);
    }

    public void ReconcileOfflineChanges(SourceMapping mapping)
    {
        while (_changeQueue.Count > 0)
        {
            FileSystemEvent? change = _changeQueue.Dequeue();
            if (change != null)
            {
                ProcessChange(mapping, change);
            }
        }
    }
}
