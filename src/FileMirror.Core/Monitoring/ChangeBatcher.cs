using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FileMirror.Core.Config;

namespace FileMirror.Core.Monitoring;

public class ChangeBatcher
{
    private readonly List<FileSystemEvent> _events = new();
    private readonly TimeSpan _timeout;
    private readonly Timer _timer;
    private readonly object _lock = new();

    public ChangeBatcher(TimeSpan timeout)
    {
        _timeout = timeout;
        _timer = new Timer(OnTimer, null, Timeout.Infinite, Timeout.Infinite);
    }

    public void AddEvent(FileSystemEvent @event)
    {
        lock (_lock)
        {
            _events.Add(@event);
            ResetTimer();
        }
    }

    public List<FileSystemEvent> GetBatch()
    {
        lock (_lock)
        {
            var batch = _events.ToList();
            _events.Clear();
            return batch;
        }
    }

    private void ResetTimer()
    {
        _timer.Change((int)_timeout.TotalMilliseconds, Timeout.Infinite);
    }

    private void OnTimer(object? state)
    {
        lock (_lock)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}