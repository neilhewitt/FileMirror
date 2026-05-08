using System;
using System.Collections.Generic;
using System.IO;
using FileMirror.Core.Config;
using FileMirror.Core.Storage;
using NUnit.Framework;

namespace FileMirror.Core.Tests.Storage;

[TestFixture]
public class StateStoreTests : TestBase
{
    private StateStore _store = null!;

    [SetUp]
    public void Setup()
    {
        _store = new StateStore(StatePath);
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(StatePath))
        {
            File.Delete(StatePath);
        }
    }

    [Test]
    public void Load_NonExistentFile_CreatesNewState()
    {
        MirroredState state = _store.Load();

        Assert.That(state.Files, Is.Not.Null);
        Assert.That(state.Files.Count, Is.EqualTo(0));
    }

    [Test]
    public void Load_ExistingFile_ParsesState()
    {
        FileState fileState = new()
        {
            Path = SourcePath + "\\test.txt",
            LastModified = DateTime.Now,
            Length = 100,
            Attributes = FileAttributes.Normal,
            IsDead = false
        };
        _store.UpdateFileState(SourcePath + "\\test.txt", fileState);
        _store.Save();

        StateStore store2 = new(StatePath);
        MirroredState loadedState = store2.Load();

        Assert.That(loadedState.Files.Count, Is.EqualTo(1));
        Assert.That(loadedState.Files.ContainsKey(SourcePath + "\\test.txt"));
    }

    [Test]
    public void Save_WritesJsonToFile()
    {
        _store.Save();

        Assert.That(File.Exists(StatePath), Is.True);
    }

    [Test]
    public void GetFileState_FoundReturnsState()
    {
      FileState fileState = new()
        {
            Path = SourcePath + "\\test.txt",
            LastModified = DateTime.Now,
            Length = 100,
            Attributes = FileAttributes.Normal
        };
        _store.UpdateFileState(SourcePath + "\\test.txt", fileState);

        FileState? result = _store.GetFileState(SourcePath + "\\test.txt");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Path, Is.EqualTo(SourcePath + "\\test.txt"));
    }

    [Test]
    public void GetFileState_NotFoundReturnsNull()
    {
        FileState? result = _store.GetFileState(SourcePath + "\\nonexistent.txt");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void UpdateFileState_AddsNewFile()
    {
     FileState fileState = new()
        {
            Path = SourcePath + "\\test.txt",
            LastModified = DateTime.Now,
            Length = 100,
            Attributes = FileAttributes.Normal
        };
        _store.UpdateFileState(SourcePath + "\\test.txt", fileState);

        FileState? result = _store.GetFileState(SourcePath + "\\test.txt");

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void UpdateFileState_UpdatesExistingFile()
    {
        FileState fileState1 = new()
        {
            Path = SourcePath + "\\test.txt",
            LastModified = DateTime.Now.AddMinutes(-1),
            Length = 100,
            Attributes = FileAttributes.Normal
        };
        _store.UpdateFileState(SourcePath + "\\test.txt", fileState1);

        FileState fileState2 = new()
        {
            Path = SourcePath + "\\test.txt",
            LastModified = DateTime.Now,
            Length = 200,
            Attributes = FileAttributes.Archive
        };
        _store.UpdateFileState(SourcePath + "\\test.txt", fileState2);

        FileState? result = _store.GetFileState(SourcePath + "\\test.txt");

        Assert.That(result!.Length, Is.EqualTo(200));
    }

    [Test]
    public void MarkDead_SetsIsDeadFlag()
    {
        FileState fileState = new()
        {
            Path = SourcePath + "\\test.txt",
            LastModified = DateTime.Now,
            Length = 100,
            Attributes = FileAttributes.Normal
        };
        _store.UpdateFileState(SourcePath + "\\test.txt", fileState);

        _store.MarkDead(SourcePath + "\\test.txt");

        FileState? result = _store.GetFileState(SourcePath + "\\test.txt");

        Assert.That(result!.IsDead, Is.True);
    }

    [Test]
    public void GetDeadPaths_ReturnsDeadPaths()
    {
        FileState fileState1 = new()
        {
            Path = SourcePath + "\\dead.txt",
            LastModified = DateTime.Now,
            Length = 100,
            Attributes = FileAttributes.Normal,
            IsDead = true
        };
        _store.UpdateFileState(SourcePath + "\\dead.txt", fileState1);

        FileState fileState2 = new()
        {
            Path = SourcePath + "\\alive.txt",
            LastModified = DateTime.Now,
            Length = 100,
            Attributes = FileAttributes.Normal
        };
        _store.UpdateFileState(SourcePath + "\\alive.txt", fileState2);

        List<string> deadPaths = _store.GetDeadPaths();

        Assert.That(deadPaths.Count, Is.EqualTo(1));
        Assert.That(deadPaths[0], Is.EqualTo(SourcePath + "\\dead.txt"));
    }
}
