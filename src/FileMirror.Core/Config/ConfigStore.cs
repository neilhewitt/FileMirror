using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace FileMirror.Core.Config;

public class ConfigStore
{
    public Config Load(string path)
    {
        if (!File.Exists(path))
        {
            return new Config();
        }

        string json = File.ReadAllText(path);
        ConfigParser parser = new();
        Config config = parser.Parse(json);

        return config;
    }

    public void Save(string path, Config config)
    {
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        var obj = new
        {
            sourceMappings = config.SourceMappings.Select(m => new
            {
                sourcePath = m.SourcePath,
                targetPath = m.TargetPath,
                recursive = m.Recursive
            }),
            reloadInterval = config.ReloadInterval.ToString()
        };

        string json = JsonConvert.SerializeObject(obj, settings);
        File.WriteAllText(path, json);
    }

    public void Watch(string path, Action<string> onChange)
    {
        string directory = Path.GetDirectoryName(path);
        string fileName = Path.GetFileName(path);
        
        if (string.IsNullOrEmpty(directory))
            return;

        FileSystemWatcher watcher = new(directory)
        {
            Filter = fileName,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true
        };

        watcher.Changed += (_, e) =>
        {
            if (Path.GetFileName(e.FullPath) == fileName)
            {
                onChange(e.FullPath);
            }
        };
    }
}