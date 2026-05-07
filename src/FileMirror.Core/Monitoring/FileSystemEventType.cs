using System;

namespace FileMirror.Core.Monitoring;

public enum FileSystemEventType
{
    Changed,
    Created,
    Deleted,
    Renamed,
    Error
}