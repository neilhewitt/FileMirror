using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using FileMirror.Core.Config;
using FileMirror.Core.Monitoring;

namespace FileMirror.Service;

public class FileMirrorService : ServiceBase, IDisposable
{
    private Config _config = null!;
    private ConfigStore _configStore = null!;
    private FileSystemWatcherWrapper[] _watchers = null!;
    private bool _disposed = false;

    public FileMirrorService()
    {
        ServiceName = "FileMirror";
    }

    protected override void OnStart(string[] args)
    {
        string configPath = args.Length > 0 ? args[0] : "config.ini";

        _configStore = new ConfigStore();
        _configStore.Watch(configPath, (path) =>
        {
            Console.WriteLine("Config file changed, reloading...");
            _configStore.Save(path, _config);
            Config newConfig = _configStore.Load(path);
            if (newConfig.SourceMappings.Count != _config.SourceMappings.Count)
            {
                Console.WriteLine($"Config changed: {_config.SourceMappings.Count} -> {newConfig.SourceMappings.Count} mappings");
            }
            _config = newConfig;
            ReinitializeWatchers();
        });

        _config = _configStore.Load(configPath);

        _watchers = new FileSystemWatcherWrapper[_config.SourceMappings.Count];

        for (int i = 0; i < _config.SourceMappings.Count; i++)
        {
            SourceMapping mapping = _config.SourceMappings[i];
            FileSystemWatcherWrapper wrapper = new(mapping);
            wrapper.Start();
            _watchers[i] = wrapper;
        }
    }

    protected override void OnStop()
    {
        if (_watchers != null)
        {
            foreach (FileSystemWatcherWrapper wrapper in _watchers)
            {
                wrapper.Stop();
            }
            _watchers = null;
        }
    }

    protected override void OnContinue()
    {
    }

    protected override void OnPause()
    {
    }

    private void ReinitializeWatchers()
    {
        if (_watchers != null)
        {
            foreach (FileSystemWatcherWrapper wrapper in _watchers)
            {
                wrapper.Stop();
            }
        }

        _watchers = new FileSystemWatcherWrapper[_config.SourceMappings.Count];

        for (int i = 0; i < _config.SourceMappings.Count; i++)
        {
            SourceMapping mapping = _config.SourceMappings[i];
            FileSystemWatcherWrapper wrapper = new(mapping);
            wrapper.Start();
            _watchers[i] = wrapper;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _watchers?.ToList().ForEach(w => w.Stop());
            _watchers = null;

            _configStore?.Dispose();
            _configStore = null;

            _disposed = true;
        }
    }
}
