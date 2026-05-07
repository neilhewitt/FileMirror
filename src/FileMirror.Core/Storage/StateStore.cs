using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileMirror.Core.Config;
using FileMirror.Core.Monitoring;
using Newtonsoft.Json;

namespace FileMirror.Core.Storage;

public class StateStore
{
    private readonly string _statePath;
    private MirroredState _state = new();

    public StateStore(string statePath)
    {
        _statePath = statePath;
    }

    public MirroredState Load()
    {
        if (!File.Exists(_statePath))
        {
            _state = new MirroredState();
            return _state;
        }

        string json = File.ReadAllText(_statePath);
        _state = JsonConvert.DeserializeObject<MirroredState>(json) ?? new MirroredState();

        return _state;
    }

    public void Save()
    {
        string json = JsonConvert.SerializeObject(_state, Formatting.Indented);
        File.WriteAllText(_statePath, json);
    }

    public FileState? GetFileState(string sourcePath)
    {
        return _state.Files.TryGetValue(sourcePath, out FileState? fileState) ? fileState : null;
    }

    public void UpdateFileState(string sourcePath, FileState state)
    {
        _state.Files[sourcePath] = state;
    }

    public void MarkDead(string sourcePath)
    {
        if (_state.Files.TryGetValue(sourcePath, out FileState? fileState) && fileState != null)
        {
            fileState.IsDead = true;
            _state.Files[sourcePath] = fileState;
        }
    }

    public List<string> GetDeadPaths()
    {
        return _state.Files.Where(kv => kv.Value.IsDead).Select(kv => kv.Key).ToList();
    }
}
