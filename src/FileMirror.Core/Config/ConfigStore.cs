using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileMirror.Core.Config;

public class ConfigStore : IDisposable
{
    private FileSystemWatcher? _watcher;

    public Config Load(string path)
    {
        if (!File.Exists(path))
        {
            return new Config();
        }

        string[] lines = File.ReadAllLines(path);
        Config config = new();
        string currentSection = null;

        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(";") || trimmed.StartsWith("#"))
            {
                continue;
            }

            if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
            {
                currentSection = trimmed.Substring(1, trimmed.Length - 2);
                continue;
            }

            if (string.IsNullOrEmpty(currentSection))
            {
                continue;
            }

            if (trimmed.Contains("="))
            {
                int eqIndex = trimmed.IndexOf('=');
                string key = trimmed.Substring(0, eqIndex).Trim();
                string value = trimmed.Substring(eqIndex + 1).Trim();

                if (currentSection == "config")
                {
                    if (key == "reloadInterval" && TimeSpan.TryParse(value, out TimeSpan reloadInterval))
                    {
                        config.ReloadInterval = reloadInterval;
                    }
                    if (key == "batchChanges" && bool.TryParse(value, out bool batchChanges))
                    {
                        config.BatchChanges = batchChanges;
                    }
                }
                else if (currentSection.StartsWith("mapping["))
                {
                    if (key == "sourcePath")
                    {
                        config.SourceMappings.Add(new SourceMapping { SourcePath = value });
                    }
                    if (key == "targetPath" && config.SourceMappings.Count > 0)
                    {
                        config.SourceMappings.Last().TargetPath = value;
                    }
                    if (key == "recursive" && config.SourceMappings.Count > 0)
                    {
                        bool.TryParse(value, out bool recursive);
                        config.SourceMappings.Last().Recursive = recursive;
                    }
                }
            }
        }

        return config;
    }

    public void Save(string path, Config config)
    {
        List<string> lines = new();

        lines.Add("; FileMirror configuration");
        lines.Add("; Source paths are mirrored one-way to target paths");
        lines.Add("");

        lines.Add("[config]");
        if (config.ReloadInterval != TimeSpan.FromSeconds(5))
        {
            lines.Add($"reloadInterval = {config.ReloadInterval}");
        }
        if (!config.BatchChanges)
        {
            lines.Add($"batchChanges = {config.BatchChanges}");
        }

        if (config.SourceMappings.Count > 0)
        {
            lines.Add("");
            lines.Add("; Source mappings - each maps a source directory to a target directory");
            lines.Add("; Set recursive to true to mirror subdirectories");
            lines.Add("");

            for (int i = 0; i < config.SourceMappings.Count; i++)
            {
                SourceMapping mapping = config.SourceMappings[i];
                lines.Add($"[mapping[{i}]]");
                lines.Add($"sourcePath = {mapping.SourcePath}");
                lines.Add($"targetPath = {mapping.TargetPath}");
                if (!mapping.Recursive)
                {
                    lines.Add($"recursive = {mapping.Recursive}");
                }
                lines.Add("");
            }
        }

        File.WriteAllLines(path, lines);
    }

    public void Watch(string path, Action<string> onChange)
    {
        string directory = Path.GetDirectoryName(path);
        string fileName = Path.GetFileName(path);
        
        if (string.IsNullOrEmpty(directory))
            return;

        _watcher?.Dispose();
        _watcher = new FileSystemWatcher(directory)
        {
            Filter = fileName,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true
        };

        _watcher.Changed += (_, e) =>
        {
            if (Path.GetFileName(e.FullPath) == fileName)
            {
                onChange(e.FullPath);
            }
        };
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _watcher = null;
    }

    public List<string> Validate(Config config)
    {
        ConfigParser parser = new();
        return parser.Validate(config);
    }
}
