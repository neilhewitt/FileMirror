using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileMirror.Core.Config;
using FileMirror.Core.Mirroring;
using FileMirror.Core.Monitoring;

namespace FileMirror;

public static class Program
{
    private const string VERSION = "1.0.0";

    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("FileMirror - File mirroring tool");

        var configOption = new Option<string>("--config", () => GetDefaultConfigPath(), "Path to config file (defaults to config.ini in same folder as executable)");
        var foregroundOption = new Option<bool>("--foreground", "Run in foreground mode");
        var scaffoldOption = new Option<bool>("--scaffold", "Create a default config file if it doesn't exist");
        var addMappingOption = new Option<string>("--add-mapping", "Add a source->target mapping (format: source|target|recursive)");
        var listMappingsOption = new Option<bool>("--list-mappings", "List all source->target mappings");

        rootCommand.AddOption(configOption);
        rootCommand.AddOption(foregroundOption);
        rootCommand.AddOption(scaffoldOption);
        rootCommand.AddOption(addMappingOption);
        rootCommand.AddOption(listMappingsOption);

        rootCommand.SetHandler(async (string configPath, bool foreground, bool scaffold, string? addMapping, bool listMappings) =>
        {
            ConfigStore configStore = new();

            Config config;
        if (File.Exists(configPath))
        {
            config = configStore.Load(configPath);
        }
        else if (scaffold)
        {
            WriteScaffoldConfig(configPath);
            Console.WriteLine($"Created default config file: {configPath}");
            return;
        }
        else
        {
            Console.WriteLine($"Error: Config file not found: {configPath}");
            if (Console.IsInputRedirected)
            {
                return;
            }
            Console.WriteLine("Would you like to create a default config file? [y/n]");
            ConsoleKey key = Console.ReadKey(false).Key;
    if (key == ConsoleKey.Y)
        {
            WriteScaffoldConfig(configPath);
            Console.WriteLine($"\nCreated default config file: {configPath}");
            return;
        }
            Console.WriteLine("\nOperation cancelled.");
            return;
        }

     if (addMapping != null)
        {
            string[] parts = addMapping.Split('|');
            if (parts.Length != 3)
            {
                Console.WriteLine("Error: --add-mapping requires format: source|target|recursive");
                return;
            }

            string sourcePath = parts[0];
            string targetPath = parts[1];
            bool recursive = bool.Parse(parts[2]);

                config.SourceMappings.Add(new SourceMapping(sourcePath, targetPath, recursive));
                configStore.Save(configPath, config);
                Console.WriteLine($"Added mapping: {sourcePath} -> {targetPath} (recursive: {recursive})");
                return;
            }

            if (listMappings)
            {
                if (config.SourceMappings.Count == 0)
                {
                    Console.WriteLine("No mappings configured.");
                }
                else
                {
                    Console.WriteLine("Configured mappings:");
                    foreach (SourceMapping mapping in config.SourceMappings)
                    {
                        Console.WriteLine($"  {mapping.SourcePath} -> {mapping.TargetPath} (recursive: {mapping.Recursive})");
                    }
                }
                return;
            }

            List<string> errors = configStore.Validate(config);

            if (errors.Count > 0)
            {
                Console.WriteLine("Configuration errors:");
                foreach (string error in errors)
                {
                    Console.WriteLine($"  - {error}");
                }
                return;
            }

            if (foreground)
            {
                await RunForeground(config, configPath);
            }
            else
            {
                await RunBackground(config, configPath);
            }
        }, configOption, foregroundOption, scaffoldOption, addMappingOption, listMappingsOption);

        return await rootCommand.InvokeAsync(args);
    }

   private static string GetDefaultConfigPath()
    {
        string executablePath = Environment.GetCommandLineArgs()[0];
        string executableDir = Path.GetDirectoryName(executablePath) ?? Directory.GetCurrentDirectory();
        return Path.Combine(executableDir, "config.ini");
    }

  private static void WriteScaffoldConfig(string path)
    {
        string executableDir = Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory();
        string parentDir = Directory.GetParent(executableDir)?.FullName ?? executableDir;
        string sourcePath = Path.Combine(parentDir, "Source");
        string targetPath = Path.Combine(parentDir, "Target");

        string scaffoldContent = $@"; FileMirror configuration
; Source paths are mirrored one-way to target paths

[config]
; How often to check for config changes (default: 5 seconds)
;reloadInterval = 00:00:05
;batchChanges = true

; Source mappings - each maps a source directory to a target directory
; Set recursive to true to mirror subdirectories
[mapping[0]]
sourcePath = {sourcePath}
targetPath = {targetPath}
recursive = true

; Example for a second mapping
; [mapping[1]]
; sourcePath = E:\data\projects
; targetPath = F:\backup\projects
; recursive = false
";
        File.WriteAllText(path, scaffoldContent);
    }

    private static async Task RunForeground(Config config, string configPath)
    {
        Console.WriteLine("Starting FileMirror in foreground mode...");
        Console.WriteLine("Press Ctrl+C to stop.");

        using CancellationTokenSource cts = new();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        using ConfigStore configStore = new();
        configStore.Watch(configPath, (path) =>
        {
            Console.WriteLine("Config file changed, reloading...");
            Config newConfig = configStore.Load(path);
            if (newConfig.SourceMappings.Count != config.SourceMappings.Count)
            {
                Console.WriteLine($"Config changed: {config.SourceMappings.Count} -> {newConfig.SourceMappings.Count} mappings");
            }
            config = newConfig;
        });

        FileMirrorEngine engine = new();
        FileSystemWatcherWrapper[] watchers = new FileSystemWatcherWrapper[config.SourceMappings.Count];

        for (int i = 0; i < config.SourceMappings.Count; i++)
        {
            SourceMapping mapping = config.SourceMappings[i];
            FileSystemWatcherWrapper wrapper = new(mapping);
            wrapper.OnFileChanged += (e) => engine.ProcessChange(mapping, e);
            wrapper.OnErrorLogged += (msg) => Console.WriteLine($"Watcher error: {msg}");
            wrapper.Start();
            watchers[i] = wrapper;

            SyncExistingFiles(mapping, engine);
        }

        Task periodicSync = Task.Run(async () =>
        {
            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);
                foreach (SourceMapping mapping in config.SourceMappings)
                {
                    ReSyncMapping(mapping, engine);
                }
            }
        }, cts.Token);

        try
        {
            await Task.Delay(Timeout.Infinite, cts.Token);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            Console.WriteLine("Stopping FileMirror...");
            foreach (FileSystemWatcherWrapper wrapper in watchers)
            {
                wrapper.Stop();
            }
        }
    }

    private static async Task RunBackground(Config config, string configPath)
    {
        Console.WriteLine("Starting FileMirror in background mode...");

        using ConfigStore configStore = new();
        configStore.Watch(configPath, (path) =>
        {
            Console.WriteLine("Config file changed, reloading...");
            Config newConfig = configStore.Load(path);
            if (newConfig.SourceMappings.Count != config.SourceMappings.Count)
            {
                Console.WriteLine($"Config changed: {config.SourceMappings.Count} -> {newConfig.SourceMappings.Count} mappings");
            }
            config = newConfig;
        });

        FileMirrorEngine engine = new();
        FileSystemWatcherWrapper[] watchers = new FileSystemWatcherWrapper[config.SourceMappings.Count];

        for (int i = 0; i < config.SourceMappings.Count; i++)
        {
            SourceMapping mapping = config.SourceMappings[i];
            FileSystemWatcherWrapper wrapper = new(mapping);
            wrapper.OnFileChanged += (e) => engine.ProcessChange(mapping, e);
            wrapper.OnErrorLogged += (msg) => Console.WriteLine($"Watcher error: {msg}");
            wrapper.Start();
            watchers[i] = wrapper;

            SyncExistingFiles(mapping, engine);
        }

        Task periodicSync = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                foreach (SourceMapping mapping in config.SourceMappings)
                {
                    ReSyncMapping(mapping, engine);
                }
            }
        });

        try
        {
            await Task.Delay(Timeout.Infinite);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            foreach (FileSystemWatcherWrapper wrapper in watchers)
            {
                wrapper.Stop();
            }
        }
    }

  private static void ReSyncMapping(SourceMapping mapping, FileMirrorEngine engine)
    {
        try
        {
            foreach (string file in Directory.GetFiles(mapping.SourcePath, "*.*", SearchOption.AllDirectories))
            {
                string sourcePath = file;
                string targetPath = mapping.TargetPath + sourcePath[mapping.SourcePath.Length..];
                if (!File.Exists(targetPath))
                {
                    Console.WriteLine($"Re-syncing missing: {sourcePath} -> {targetPath}");
                    engine.ProcessChange(mapping, new FileSystemEvent(FileSystemEventType.Created, sourcePath));
                }
                else if (File.GetLastWriteTime(sourcePath) != File.GetLastWriteTime(targetPath) || File.GetAttributes(sourcePath) != File.GetAttributes(targetPath))
                {
                    Console.WriteLine($"Re-syncing changed: {sourcePath}");
                    engine.ProcessChange(mapping, new FileSystemEvent(FileSystemEventType.Changed, sourcePath));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Re-sync error: {ex.Message}");
        }
    }

    private static void SyncExistingFiles(SourceMapping mapping, FileMirrorEngine engine)
    {
        try
        {
            foreach (string file in Directory.GetFiles(mapping.SourcePath, "*.*", SearchOption.AllDirectories))
            {
                string sourcePath = file;
                string targetPath = mapping.TargetPath + sourcePath[mapping.SourcePath.Length..];
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                File.Copy(sourcePath, targetPath, true);
                File.SetAttributes(targetPath, File.GetAttributes(sourcePath));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error syncing existing files: {ex.Message}");
        }
    }
}
