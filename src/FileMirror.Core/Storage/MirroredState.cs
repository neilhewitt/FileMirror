using System;
using System.Collections.Generic;
using FileMirror.Core.Config;

namespace FileMirror.Core.Storage;

public class MirroredState
{
    public string SourcePath { get; set; } = "";
    public string TargetPath { get; set; } = "";
    public Dictionary<string, FileState> Files { get; } = new();
    public DateTime LastSynced { get; set; }
}