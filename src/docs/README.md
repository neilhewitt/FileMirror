# FileMirror Documentation

Welcome to FileMirror - a one-way real-time file mirroring tool.

## 🚀 Get Started (30 seconds)

```bash
# 1. Build
cd src && dotnet build -c Release && dotnet publish -c Release --self-contained true /p:PublishSingleFile=true

# 2. Create config
echo '{"sourceMappings":[{"sourcePath":"C:\\source","targetPath":"C:\\target"}]}' > config.json

# 3. Run
./FileMirror --config config.json --foreground
```

📖 **Detailed Guides:**
- **[Quick Start](quick-start.md)** - Step-by-step setup
- **[Architecture](architecture.md)** - How it works
- **[Project Structure](project-structure.md)** - Codebase layout

## 📖 Concepts

| Topic | Description | File |
|-------|-------------|------|
| **Mirroring Behavior** | One-way, real-time sync | [How It Works](how-it-works.md) |
| **Configuration** | JSON config format | [Configuration](configuration.md) |
| **State Management** | Persistence and recovery | [State Management](state-management.md) |

## 🧩 Components

| Component | Purpose | File |
|-----------|---------|------|
| **Config System** | JSON parsing, validation, hot-reload | [Config System](config-system.md) |
| **File Monitor** | FileSystemWatcher wrapper with batching | [File System Monitor](file-system-monitor.md) |
| **Mirroring Engine** | Apply source changes to target | [Mirroring Engine](mirroring-engine.md) |
| **State Persistence** | Track files, handle offline | [State Persistence](state-persistence.md) |

## ⚙️ Development

| Task | Guide |
|------|-------|
| Build and publish | [Building](building.md) |
| Run tests | [Testing](testing.md) |
| Contribute code | [Coding Style](coding-style.md), [Contributing](contributing.md) |

## 🖥️ Hosting

| Mode | Description | File |
|------|-------------|------|
| **CLI** | Command-line interface (dev/testing) | [CLI Mode](cli-mode.md) |
| **Service** | Windows Service (production) | [Windows Service](windows-service.md) |

## 🛠️ Troubleshooting

| Issue | Solution |
|-------|----------|
| Common problems | [Troubleshooting](troubleshooting.md) |
| Log output | [Logging](logging.md) |
| Release notes | [Release Notes](release-notes.md) |

## 📚 Documentation Structure

```
docs/
├── README.md                    # This file
├── quick-start.md              # Get running in 5 minutes
├── architecture.md             # System design
├── project-structure.md        # Codebase layout
├── how-it-works.md             # Core concepts
├── configuration.md            # Config file format
├── state-management.md         # Persistence and recovery
├── config-system.md            # Configuration classes
├── file-system-monitor.md      # Change detection
├── mirroring-engine.md         # Core mirroring logic
├── state-persistence.md        # State tracking
├── building.md                 # Build and publish
├── testing.md                  # Unit testing
├── coding-style.md             # Coding conventions
├── cli-mode.md                 # Command-line usage
├── windows-service.md          # Service installation
├── troubleshooting.md          # Common issues
├── logging.md                  # Log output
├── release-notes.md            # Version history
└── contributing.md             # Contribute guide
```

## 🤝 Need Help?

- **New to FileMirror?** Start with [Quick Start](quick-start.md)
- **Found a bug?** Check [Troubleshooting](troubleshooting.md)
- **Want to contribute?** Read [Contributing](contributing.md)
- **Have questions?** Open an issue on GitHub

---

**Version:** 1.0.0  
**Last Updated:** 2026-05-07
