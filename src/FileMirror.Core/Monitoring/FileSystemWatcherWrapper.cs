using System;
using System.Collections.Generic;
using System.IO;
using FileMirror.Core.Config;

namespace FileMirror.Core.Monitoring;

public class FileSystemWatcherWrapper : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly bool _recursive;
    private bool _disposed = false;

    public FileSystemWatcherWrapper(SourceMapping mapping)
    {
        _recursive = mapping.Recursive;
        _watcher = new FileSystemWatcher(mapping.SourcePath)
        {
            IncludeSubdirectories = _recursive,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size | NotifyFilters.Attributes,
            Filter = "*.*"
        };

        _watcher.Changed += OnChanged;
        _watcher.Created += OnCreated;
        _watcher.Deleted += OnDeleted;
        _watcher.Renamed += OnRenamed;
        _watcher.Error += OnError;
    }

    public event Action<FileSystemEvent>? OnFileChanged;
    public event Action<string>? OnErrorLogged;

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        OnFileChanged?.Invoke(new FileSystemEvent(e, FileSystemEventType.Changed));
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        OnFileChanged?.Invoke(new FileSystemEvent(e, FileSystemEventType.Created));
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        OnFileChanged?.Invoke(new FileSystemEvent(e, FileSystemEventType.Deleted));
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        OnFileChanged?.Invoke(new FileSystemEvent(e));
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        OnErrorLogged?.Invoke(e.GetException().Message);
    }

    public void Start() => _watcher.EnableRaisingEvents = true;
    public void Stop() => _watcher.EnableRaisingEvents = false;

    public void Dispose()
    {
        if (!_disposed)
        {
            _watcher.Dispose();
            _disposed = true;
        }
    }
}