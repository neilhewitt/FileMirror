using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace FileMirror.Core.Tests.Config;

[TestFixture]
public class ConfigStoreTests
{
    private FileMirror.Core.Config.ConfigStore _store = null!;
    private string _tempDir = null!;
    private string _configPath = null!;

    [SetUp]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _store = new FileMirror.Core.Config.ConfigStore();
        _configPath = Path.Combine(_tempDir, "config.json");
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [Test]
    public void Load_NonExistentFile_ReturnsEmptyConfig()
    {
        FileMirror.Core.Config.Config config = _store.Load(_configPath);

        Assert.That(config.SourceMappings.Count, Is.EqualTo(0));
    }

    [Test]
    public void SaveAndLoad_SavedConfig_PersistsAndReloads()
    {
        FileMirror.Core.Config.Config config = new();
        config.SourceMappings.Add(new FileMirror.Core.Config.SourceMapping("C:\\source", "C:\\target"));
        config.ReloadInterval = TimeSpan.FromSeconds(10);

        _store.Save(_configPath, config);

        FileMirror.Core.Config.Config loaded = _store.Load(_configPath);

        Assert.That(loaded.SourceMappings.Count, Is.EqualTo(1));
        Assert.That(loaded.SourceMappings[0].SourcePath, Is.EqualTo("C:\\source"));
        Assert.That(loaded.ReloadInterval, Is.EqualTo(TimeSpan.FromSeconds(10)));
    }
}