using System;
using System.Collections.Generic;

namespace FileMirror.Core.Config;

public class Config
{
    public List<SourceMapping> SourceMappings { get; } = new();
    public TimeSpan ReloadInterval { get; set; } = TimeSpan.FromSeconds(5);
    public bool BatchChanges { get; set; } = true;
}