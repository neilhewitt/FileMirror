using System.ServiceProcess;
using FileMirror.Core.Config;
using FileMirror.Core.Monitoring;

namespace FileMirror.Service;

public class FileMirrorService : ServiceBase
{
    private Config _config = null!;
    private ConfigStore _configStore = null!;
    private FileSystemWatcherWrapper[] _watchers = null!;

    public FileMirrorService()
    {
        ServiceName = "FileMirror";
    }

    protected override void OnStart(string[] args)
    {
        string configPath = args.Length > 0 ? args[0] : "config.json";

        _configStore = new ConfigStore();
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
        }
    }

    protected override void OnContinue()
    {
    }

    protected override void OnPause()
    {
    }
}
