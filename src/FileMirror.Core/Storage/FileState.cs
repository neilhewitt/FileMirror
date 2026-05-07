using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileMirror.Core.Config;
using FileMirror.Core.Monitoring;

namespace FileMirror.Core.Storage;

public class FileState
{
    public string Path { get; set; } = "";
    public DateTime LastModified { get; set; }
    public long Length { get; set; }
    public FileAttributes Attributes { get; set; }
    public bool IsDead { get; set; }
}