using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace FileMirror.Core.Tests.Config;

[TestFixture]
public class ConfigParserTests
{
    private FileMirror.Core.Config.ConfigParser _parser = null!;

    [SetUp]
    public void Setup()
    {
        _parser = new FileMirror.Core.Config.ConfigParser();
    }

    [Test]
    public void Parse_MinimalConfig_CreatesDefaultConfig()
    {
        string json = @"{ ""sourceMappings"": [] }";
        FileMirror.Core.Config.Config config = _parser.Parse(json);

        Assert.That(config.SourceMappings.Count, Is.EqualTo(0));
        Assert.That(config.ReloadInterval, Is.EqualTo(TimeSpan.FromSeconds(5)));
        Assert.That(config.BatchChanges, Is.True);
    }

    [Test]
    public void Parse_FullConfig_ParsesAllFields()
    {
        string json = @"{
            ""sourceMappings"": [
                {
                    ""sourcePath"": ""C:\\source"",
                    ""targetPath"": ""C:\\target"",
                    ""recursive"": false
                }
            ],
            ""reloadInterval"": ""00:00:10"",
            ""batchChanges"": false
        }";

        FileMirror.Core.Config.Config config = _parser.Parse(json);

        Assert.That(config.SourceMappings.Count, Is.EqualTo(1));
        Assert.That(config.SourceMappings[0].SourcePath, Is.EqualTo("C:\\source"));
        Assert.That(config.SourceMappings[0].TargetPath, Is.EqualTo("C:\\target"));
        Assert.That(config.SourceMappings[0].Recursive, Is.False);
        Assert.That(config.ReloadInterval, Is.EqualTo(TimeSpan.FromSeconds(10)));
        Assert.That(config.BatchChanges, Is.False);
    }

    [Test]
    public void Parse_MultipleMappings_ParsesAll()
    {
        string json = @"{
            ""sourceMappings"": [
                { ""sourcePath"": ""C:\\a"", ""targetPath"": ""C:\\b"" },
                { ""sourcePath"": ""C:\\c"", ""targetPath"": ""C:\\d"", ""recursive"": false }
            ]
        }";

        FileMirror.Core.Config.Config config = _parser.Parse(json);

        Assert.That(config.SourceMappings.Count, Is.EqualTo(2));
        Assert.That(config.SourceMappings[0].Recursive, Is.True);
        Assert.That(config.SourceMappings[1].Recursive, Is.False);
    }

    [Test]
    public void Validate_EmptyMappings_ReturnsError()
    {
        FileMirror.Core.Config.Config config = new();

        List<string> errors = _parser.Validate(config);

        Assert.That(errors.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Validate_ValidConfig_ReturnsEmpty()
    {
        FileMirror.Core.Config.Config config = new();
        config.SourceMappings.Add(new FileMirror.Core.Config.SourceMapping("C:\\source", "C:\\target"));

        List<string> errors = _parser.Validate(config);

        Assert.That(errors.Count, Is.EqualTo(0));
    }
}