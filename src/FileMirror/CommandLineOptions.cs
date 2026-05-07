using System;
using System.Collections.Generic;
using FileMirror.Core.Config;

namespace FileMirror;

public class CommandLineOptions
{
    public string ConfigPath { get; set; } = "";
    public bool Foreground { get; set; }
    public bool Version { get; set; }
    public bool Help { get; set; }
}