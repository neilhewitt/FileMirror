using System;
using System.Collections.Generic;
using System.Threading;
using FileMirror.Core.Monitoring;
using NUnit.Framework;

namespace FileMirror.Core.Tests.Monitoring;

[TestFixture]
public class ChangeBatcherTests
{
    private ChangeBatcher _batcher = null!;

    [SetUp]
    public void Setup()
    {
        _batcher = new ChangeBatcher(TimeSpan.FromMilliseconds(100));
    }

    [Test]
    public void AddEvent_AddsEventToBatch()
    {
        FileSystemEvent @event = new(FileSystemEventType.Created, "C:\\source\\test.txt");

        _batcher.AddEvent(@event);

        Assert.That(_batcher.GetBatch().Count, Is.EqualTo(1));
    }

    [Test]
    public void GetBatch_ReturnsAndClearsBatch()
    {
        _batcher.AddEvent(new FileSystemEvent(FileSystemEventType.Created, "C:\\source\\test1.txt"));
        _batcher.AddEvent(new FileSystemEvent(FileSystemEventType.Created, "C:\\source\\test2.txt"));

        List<FileSystemEvent> batch1 = _batcher.GetBatch();
        List<FileSystemEvent> batch2 = _batcher.GetBatch();

        Assert.That(batch1.Count, Is.EqualTo(2));
        Assert.That(batch2.Count, Is.EqualTo(0));
    }

    [Test]
    public void Timeout_FlushesBatch()
    {
        _batcher.AddEvent(new FileSystemEvent(FileSystemEventType.Created, "C:\\source\\test.txt"));

        Thread.Sleep(150);

        Assert.That(_batcher.GetBatch().Count, Is.EqualTo(1));
    }

    [Test]
    public void MultipleEvents_EventsPreserved()
    {
        _batcher.AddEvent(new FileSystemEvent(FileSystemEventType.Created, "C:\\source\\test1.txt"));
        _batcher.AddEvent(new FileSystemEvent(FileSystemEventType.Deleted, "C:\\source\\test2.txt"));

        List<FileSystemEvent> batch = _batcher.GetBatch();

        Assert.That(batch.Count, Is.EqualTo(2));
    }
}
