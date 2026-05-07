using System;
using System.IO;
using FileMirror.Core.Config;
using FileMirror.Core.Monitoring;

namespace FileMirror.Core.Mirroring;

public interface IFileMirrorOperation
{
    void ApplyToTarget(FileInfo source, FileInfo target);
    void RevertToSource(FileInfo target, FileInfo source);
}