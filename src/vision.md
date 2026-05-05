# FileMirror

## What it is, and what it does

FileMirror is a simple program, written in C# for .NET 10, but published as a framework-included single binary. It is able to be hosted in a Windows Service or 
run directly from the command line.

It monitors a set of folder paths specified in a config file, which may optionally specify that folders below it should be recursed, and mirrors any changes to them
in real time (latency <1s) to a specified path which can be either a local path or a network share path. For a network share path, FileMirror must be running on host
in order for mirroring to work - simple mirroring from client to a network path does not work.

Changes monitored and replicated include:

* Any change to file contents (to be mirrored as a diff)
* Any change to file attributes including name
* Addition of new files or folders in the path
* Removal of files or folders in the path

Files will be mirrored without security descriptors but read-only files will have their read-only flag set on the mirror.

Mirroring is \*one-way only\* from source path to target path. Changes made to the files in the target path will be reverted to reflect the current state of the source.

This includes deletions - files deleted from the target path that still exist in the source path will be re-mirrored. Changes to file contents, attributes, or name 
made on the target path for files that still exist on the source path will be overwritten (or reverted) from the source file.

If a file path is removed from the source service config file, the target path becomes 'dead' - it will no longer be mirrored to, but it will \*not\* be deleted.

If the target machine is not available for a path, mirroring will pause until it is available. All changes are eventually consistent - if multiple changes to a file are made
at the source while the target is offline, only the current file state at the source when the target comes back online will be mirrored.

FileMirror has no GUI. Configuration is entirely by config file. It can be installed as a Windows service using a PowerShell script. Changes to the config file are picked up immediately 
where possible, restarting the service should generally not be required.

## Design and architecture

The architecture should be clean and simple, with good separation of concerns. Testable, but testability should not compromise the design. We will avoid over-architecting, always bear 
YAGNI in mind, which means:

 * no factories
 * no services
 * no DDD
 * prefer concrete classes over interfaces
 * rich, functional objects
 * inheritance is fine, really
 * simple is better

 We will follow the global coding style specified in OpenCode's global AGENTS.md.









