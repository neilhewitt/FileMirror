using System;
using System.Collections.Generic;
using System.IO;
using FileMirror.Core.Config;
using FileMirror.Core.Storage;
using NUnit.Framework;

namespace FileMirror.Core.Tests.Config;

[TestFixture]
public class ConfigParserTests
{
    [Test]
    public void Load_MinimalConfig_CreatesDefaultConfig()
    {
        string configPath = Path.Combine(Path.GetTempPath(), $"FileMirrorTest_{Guid.NewGuid():N}.ini");
        string iniContent = @";
[config]
";
        File.WriteAllText(configPath, iniContent);

        FileMirror.Core.Config.ConfigStore store = new();
        FileMirror.Core.Config.Config config = store.Load(configPath);

        Assert.That(config.SourceMappings.Count, Is.EqualTo(0));
        Assert.That(config.ReloadInterval, Is.EqualTo(TimeSpan.FromSeconds(5)));
        Assert.That(config.BatchChanges, Is.True);

        File.Delete(configPath);
    }

    [Test]
    public void Load_FullConfig_ParsesAllFields()
    {
        string configPath = Path.Combine(Path.GetTempPath(), $"FileMirrorTest_{Guid.NewGuid():N}.ini");
        string sourcePath = Path.Combine(Path.GetTempPath(), "FileMirrorTestSource");
        string targetPath = Path.Combine(Path.GetTempPath(), "FileMirrorTestTarget");

        string iniContent = $@"
[config]
reloadInterval = 00:00:10
batchChanges = false

[mapping[0]]
sourcePath = {sourcePath}
targetPath = {targetPath}
recursive = false
";

        File.WriteAllText(configPath, iniContent);

        FileMirror.Core.Config.ConfigStore store = new();
        FileMirror.Core.Config.Config config = store.Load(configPath);

        Assert.That(config.SourceMappings.Count, Is.EqualTo(1));
        Assert.That(config.SourceMappings[0].SourcePath, Is.EqualTo(sourcePath));
        Assert.That(config.SourceMappings[0].TargetPath, Is.EqualTo(targetPath));
        Assert.That(config.SourceMappings[0].Recursive, Is.False);
        Assert.That(config.ReloadInterval, Is.EqualTo(TimeSpan.FromSeconds(10)));
        Assert.That(config.BatchChanges, Is.False);

        File.Delete(configPath);
    }

    [Test]
    public void Load_MultipleMappings_ParsesAll()
    {
        string configPath = Path.Combine(Path.GetTempPath(), $"FileMirrorTest_{Guid.NewGuid():N}.ini");
        string sourcePath1 = Path.Combine(Path.GetTempPath(), "FileMirrorTestA");
        string targetPath1 = Path.Combine(Path.GetTempPath(), "FileMirrorTestB");
        string sourcePath2 = Path.Combine(Path.GetTempPath(), "FileMirrorTestC");
        string targetPath2 = Path.Combine(Path.GetTempPath(), "FileMirrorTestD");

        string iniContent = $@"
[mapping[0]]
sourcePath = {sourcePath1}
targetPath = {targetPath1}

[mapping[1]]
sourcePath = {sourcePath2}
targetPath = {targetPath2}
recursive = false
";

        File.WriteAllText(configPath, iniContent);

        FileMirror.Core.Config.ConfigStore store = new();
        FileMirror.Core.Config.Config config = store.Load(configPath);

        Assert.That(config.SourceMappings.Count, Is.EqualTo(2));
        Assert.That(config.SourceMappings[0].Recursive, Is.True);
        Assert.That(config.SourceMappings[1].Recursive, Is.False);

        File.Delete(configPath);
    }

    [Test]
    public void Validate_EmptyMappings_ReturnsError()
    {
        FileMirror.Core.Config.Config config = new();

        FileMirror.Core.Config.ConfigParser parser = new();
        List<string> errors = parser.Validate(config);

        Assert.That(errors.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Validate_ValidConfig_ReturnsEmpty()
    {
        FileMirror.Core.Config.Config config = new();
        string sourcePath = Path.Combine(Path.GetTempPath(), "FileMirrorTestSource");
        string targetPath = Path.Combine(Path.GetTempPath(), "FileMirrorTestTarget");
        config.SourceMappings.Add(new FileMirror.Core.Config.SourceMapping(sourcePath, targetPath));

        FileMirror.Core.Config.ConfigParser parser = new();
        List<string> errors = parser.Validate(config);

        Assert.That(errors.Count, Is.EqualTo(0));
    }
}
