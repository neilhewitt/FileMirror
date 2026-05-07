namespace FileMirror.Core.Config;

public class SourceMapping
{
    public string SourcePath { get; set; } = null!;
    public string TargetPath { get; set; } = null!;
    public bool Recursive { get; set; }
    public bool IsDead { get; private set; }

    public SourceMapping()
    {
        Recursive = true;
        IsDead = false;
    }

    public SourceMapping(string sourcePath, string targetPath, bool recursive = true)
    {
        SourcePath = sourcePath;
        TargetPath = targetPath;
        Recursive = recursive;
        IsDead = false;
    }

    public void MarkDead() => IsDead = true;
}