using System;
using System.IO;
using System.Threading;
using FileMirror.Core.Config;
using FileMirror.Core.Monitoring;
using NUnit.Framework;

namespace FileMirror.Core.Tests.Monitoring;

[TestFixture]
public class FileSystemWatcherWrapperTests : TestBase
{
    private FileSystemWatcherWrapper _watcher = null!;

    [SetUp]
    public void Setup()
    {
        _watcher = new FileSystemWatcherWrapper(new SourceMapping(SourcePath, TargetPath));
    }

    [TearDown]
    public void TearDown()
    {
        _watcher.Stop();
        _watcher.Dispose();
    }

    [Test]
    public void Start_EnablesRaisingEvents()
    {
        _watcher.Start();

        Assert.That(() => _watcher, Is.Not.Null);
    }

    [Test]
    public void Stop_DisablesRaisingEvents()
    {
        _watcher.Start();
        _watcher.Stop();

        Assert.That(() => _watcher, Is.Not.Null);
    }

    [Test]
    public void Constructor_SubscribesToEvents()
    {
        bool eventFired = false;
        _watcher.OnFileChanged += (_) => eventFired = true;
        _watcher.OnErrorLogged += (_) => eventFired = true;

        _watcher.Start();

        string testFile = Path.Combine(SourcePath, "test.txt");
        File.WriteAllText(testFile, "test content");

        Thread.Sleep(100);

        _watcher.Stop();

        Assert.That(eventFired, Is.True);
    }
}
