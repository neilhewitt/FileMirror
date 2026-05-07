using System;
using System.Collections.Generic;
using FileMirror.Core.Monitoring;

namespace FileMirror.Core.Storage;

public class ChangeQueue
{
    private readonly Queue<FileSystemEvent> _queue = new();

    public void Enqueue(FileSystemEvent change)
    {
        _queue.Enqueue(change);
    }

    public FileSystemEvent? Dequeue()
    {
        return _queue.Count > 0 ? _queue.Dequeue() : (FileSystemEvent?)null;
    }

    public FileSystemEvent? Peek()
    {
        return _queue.Count > 0 ? _queue.Peek() : (FileSystemEvent?)null;
    }

    public int Count => _queue.Count;
}