using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileMirror.Core.Config;
using FileMirror.Core.Monitoring;

namespace FileMirror;

public static class Program
{
    private const string VERSION = "1.0.0";

    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("FileMirror - File mirroring tool");

        var configOption = new Option<string>("--config", "Path to config file (required)") { IsRequired = true };
        var foregroundOption = new Option<bool>("--foreground", "Run in foreground mode");
        var versionOption = new Option<bool>("--version", "Show version information");
        var helpOption = new Option<bool>("--help", "Show this help message");

        rootCommand.AddOption(configOption);
        rootCommand.AddOption(foregroundOption);
        rootCommand.AddOption(versionOption);
        rootCommand.AddOption(helpOption);

        rootCommand.SetHandler(async (string configPath, bool foreground, bool version, bool help) =>
        {
            if (version)
            {
                Console.WriteLine($"FileMirror version {VERSION}");
                return;
            }

            if (help)
            {
                return;
            }

            if (!File.Exists(configPath))
            {
                Console.WriteLine($"Error: Config file not found: {configPath}");
                return;
            }

            ConfigStore configStore = new();
            Config config = configStore.Load(configPath);
            ConfigParser parser = new();
            List<string> errors = parser.Validate(config);

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
                await RunForeground(config);
            }
            else
            {
                await RunBackground(config);
            }
        }, configOption, foregroundOption, versionOption, helpOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task RunForeground(Config config)
    {
        Console.WriteLine("Starting FileMirror in foreground mode...");
        Console.WriteLine("Press Ctrl+C to stop.");

        using CancellationTokenSource cts = new();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        FileSystemWatcherWrapper[] watchers = new FileSystemWatcherWrapper[config.SourceMappings.Count];

        for (int i = 0; i < config.SourceMappings.Count; i++)
        {
            SourceMapping mapping = config.SourceMappings[i];
            FileSystemWatcherWrapper wrapper = new(mapping);
            wrapper.Start();
            watchers[i] = wrapper;
        }

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

    private static async Task RunBackground(Config config)
    {
        Console.WriteLine("Starting FileMirror in background mode...");

        FileSystemWatcherWrapper[] watchers = new FileSystemWatcherWrapper[config.SourceMappings.Count];

        for (int i = 0; i < config.SourceMappings.Count; i++)
        {
            SourceMapping mapping = config.SourceMappings[i];
            FileSystemWatcherWrapper wrapper = new(mapping);
            wrapper.Start();
            watchers[i] = wrapper;
        }

        try
        {
            await Task.Delay(Timeout.Infinite);
        }
        catch (Exception)
        {
        }
        finally
        {
            foreach (FileSystemWatcherWrapper wrapper in watchers)
            {
                wrapper.Stop();
            }
        }
    }
}