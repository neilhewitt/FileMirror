using System;
using System.Collections.Generic;
using System.IO;
using FileMirror.Core.Monitoring;
using FileMirror.Core.Storage;
using NUnit.Framework;

namespace FileMirror.Core.Tests.Storage;

[TestFixture]
public class ChangeQueueTests
{
    private ChangeQueue _queue = null!;
    private string _sourcePath = null!;

    [SetUp]
    public void Setup()
    {
        _queue = new ChangeQueue();
        _sourcePath = Path.Combine(Path.GetTempPath(), $"FileMirrorTest_{Guid.NewGuid():N}", "source");
        Directory.CreateDirectory(_sourcePath);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_sourcePath))
        {
            Directory.Delete(_sourcePath, true);
        }
    }

    [Test]
    public void Enqueue_IncrementsCount()
    {
        _queue.Enqueue(new FileSystemEvent(FileSystemEventType.Created, _sourcePath + "\\test.txt"));

        Assert.That(_queue.Count, Is.EqualTo(1));
    }

    [Test]
    public void Enqueue_Dequeue_FIFOOrder()
    {
        FileSystemEvent event1 = new(FileSystemEventType.Created, _sourcePath + "\\test1.txt");
        FileSystemEvent event2 = new(FileSystemEventType.Created, _sourcePath + "\\test2.txt");

        _queue.Enqueue(event1);
        _queue.Enqueue(event2);

        FileSystemEvent? result1 = _queue.Dequeue();
        FileSystemEvent? result2 = _queue.Dequeue();

        Assert.That(result1!.Type, Is.EqualTo(FileSystemEventType.Created));
        Assert.That(result1!.Path, Is.EqualTo(_sourcePath + "\\test1.txt"));
        Assert.That(result2!.Type, Is.EqualTo(FileSystemEventType.Created));
        Assert.That(result2!.Path, Is.EqualTo(_sourcePath + "\\test2.txt"));
    }

    [Test]
    public void Dequeue_EmptyQueue_ReturnsNull()
    {
        FileSystemEvent? result = _queue.Dequeue();

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Peek_ReturnsWithoutRemoving()
    {
        FileSystemEvent @event = new(FileSystemEventType.Created, _sourcePath + "\\test.txt");
        _queue.Enqueue(@event);

        FileSystemEvent? peeked = _queue.Peek();
        FileSystemEvent? dequeued = _queue.Dequeue();
        FileSystemEvent? peekedAgain = _queue.Peek();

        Assert.That(peeked!.Path, Is.EqualTo(_sourcePath + "\\test.txt"));
        Assert.That(dequeued!.Path, Is.EqualTo(_sourcePath + "\\test.txt"));
        Assert.That(peekedAgain, Is.Null);
    }
}
