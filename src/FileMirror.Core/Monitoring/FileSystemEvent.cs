using System;
using System.IO;

namespace FileMirror.Core.Monitoring;

public class FileSystemEvent
{
    public FileSystemEventType Type { get; }
    public string Path { get; }
    public string? OldPath { get; }
    public DateTime Timestamp { get; }

    public FileSystemEvent(FileSystemEventType type, string path, string? oldPath = null)
    {
        Type = type;
        Path = path;
        OldPath = oldPath;
        Timestamp = DateTime.Now;
    }

    public FileSystemEvent(FileSystemEventArgs args, FileSystemEventType type)
        : this(type, args.FullPath)
    {
    }

    public FileSystemEvent(RenamedEventArgs args)
        : this(FileSystemEventType.Renamed, args.FullPath, args.OldFullPath)
    {
    }
}