namespace FileMirror.Core.Config;

public class SourceMapping
{
    public string SourcePath { get; set; } = null!;
    public string TargetPath { get; set; } = null!;
    public bool Recursive { get; set; }

    public SourceMapping()
    {
        Recursive = true;
    }

    public SourceMapping(string sourcePath, string targetPath, bool recursive = true)
    {
        SourcePath = sourcePath;
        TargetPath = targetPath;
        Recursive = recursive;
    }
}