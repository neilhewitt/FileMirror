using System;
using System.IO;
using FileMirror.Core.Config;

namespace FileMirror.Core.Mirroring;

public class RevertEngine
{
    public void DetectAndRevert(FileInfo source, FileInfo target)
    {
        if (!source.Exists)
        {
            return;
        }

        if (!target.Exists)
        {
            return;
        }

        if (source.LastWriteTime != target.LastWriteTime || source.Length != target.Length)
        {
            source.CopyTo(target.FullName, true);
            File.SetAttributes(target.FullName, source.Attributes);
        }
        else if (target.Attributes != source.Attributes)
        {
            File.SetAttributes(target.FullName, source.Attributes);
        }
    }

    public void DetectAndRevert(DirectoryInfo source, DirectoryInfo target)
    {
        if (!source.Exists)
        {
            return;
        }

        if (!target.Exists)
        {
            return;
        }

        if (source.Attributes != target.Attributes)
        {
            Directory.SetLastWriteTime(target.FullName, source.LastWriteTime);
        }
    }
}
