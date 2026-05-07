using System;
using System.IO;
using FileMirror.Core.Config;
using FileMirror.Core.Monitoring;

namespace FileMirror.Core.Mirroring;

public interface IDirectoryMirrorOperation
{
    void Create(DirectoryInfo source, DirectoryInfo target);
    void Delete(DirectoryInfo target);
    void SyncAttributes(DirectoryInfo source, DirectoryInfo target);
}