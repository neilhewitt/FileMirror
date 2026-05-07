using System;
using System.IO;

namespace FileMirror.Core.Tests;

public abstract class TestBase : IDisposable
{
    private const string TestEnvVar = "FILEMIRROR_TEST_PATH";

    protected string TestBasePath { get; }
    protected string SourcePath { get; }
    protected string TargetPath { get; }
    protected string StatePath { get; }

    private bool _disposed = false;

    protected TestBase(string? testBasePathOverride = null)
    {
        TestBasePath = testBasePathOverride ?? GetTestBasePath();
        SourcePath = Path.Combine(TestBasePath, "source");
        TargetPath = Path.Combine(TestBasePath, "target");
        StatePath = Path.Combine(TestBasePath, "state.json");

        Directory.CreateDirectory(SourcePath);
        Directory.CreateDirectory(TargetPath);
    }

    private static string GetTestBasePath()
    {
        string? envPath = Environment.GetEnvironmentVariable(TestEnvVar);
        if (!string.IsNullOrWhiteSpace(envPath))
        {
            ValidateTestPath(envPath);
            return envPath;
        }

        return GetDefaultTestBasePath();
    }

    private static void ValidateTestPath(string path)
    {
        if (!Path.IsPathRooted(path))
        {
            throw new ArgumentException("Test path must be an absolute path", nameof(path));
        }

        string repoRoot = GetRepoRoot();
        if (path.StartsWith(repoRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Test path cannot be inside the repository", nameof(path));
        }
    }

    private static string GetRepoRoot()
    {
        string currentDir = Directory.GetCurrentDirectory();
        
        while (currentDir.Length > 3)
        {
            if (File.Exists(Path.Combine(currentDir, "AGENTS.md")))
            {
                return currentDir;
            }

            DirectoryInfo dirInfo = new DirectoryInfo(currentDir);
            if (dirInfo.Parent == null)
            {
                break;
            }

            currentDir = dirInfo.Parent.FullName;
        }

        return Directory.GetCurrentDirectory();
    }

    private static string GetDefaultTestBasePath()
    {
        string tempPath = Path.GetTempPath();
        string folderName = $"FileMirrorTest_{Guid.NewGuid():N}";
        return Path.Combine(tempPath, folderName);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            try
            {
                if (Directory.Exists(TestBasePath))
                {
                    Directory.Delete(TestBasePath, true);
                }
            }
            catch
            {
            }

            _disposed = true;
        }
    }
}
