# SPT CheckMods

> **Note:** This is an experimental, AI-assisted fork of Refringe's excellent `SPT-Check-Mods`. I am adding custom features for my own workflow. I will not be submitting Pull Requests upstream to respect the original author's non-AI development process, but anyone is welcome to use or borrow ideas from this fork!

A robust, lightning-fast .NET 9.0 command-line application for verifying, validating, and updating mods for Single Player Tarkov (SPT). 

By analyzing local mod metadata and communicating securely with the SPT Forge API, CheckMods ensures your installation is up-to-date, structurally valid, and dependency-compliant.

## Features

- **Deep Metadata Extraction:** Uses reflection and custom assembly load contexts to read metadata (GUID, version, name) directly from server mods (`user/mods/**/*.dll`) and client plugins (`BepInEx/plugins/**/*.dll`).
- **Misplacement & Structure Detection:** Intelligently flags incorrectly installed mods (e.g., client DLLs placed in server directories) and detects "cross-installed" directories where unrelated mods share the same subfolder.
- **Smart Mod Grouping:** Automatically groups multi-DLL client mods by analyzing assembly references and author namespaces.
- **Authoritative API Validation:** Securely queries the [SPT Forge API](https://forge.sp-tarkov.com/) (using exact GUIDs or fuzzy-name matching) to verify mod legitimacy and fetch update metadata.
- **Dependency & Compatibility Checking:** Resolves recursive dependency trees and verifies that your installed mods are strictly compatible with your local SPT version.
- **Smart Ignore Lists:** Allows users to suppress false-positive updates locally, and fetches remote, author-maintained ignore lists for seamless validation.
- **Performance Optimized:** Employs built-in rate-limiting, exponential backoff, memory caching, and parallel processing for optimal API interactions.

## Prerequisites

To build or run CheckMods from source, you will need:
- [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) (or Runtime if running published binaries).

## Usage

CheckMods is compiled as a standalone, single-file executable. Run it from your terminal, passing the path to your SPT installation as the only argument:

```bash
# Example (Windows)
CheckMods.exe "C:\Path\To\SPT"

# Example (Cross-platform/Source)
dotnet run -- "C:\Path\To\SPT"
```

The application provides a rich, interactive CLI experience via `Spectre.Console`. 

## Configuration & Logs

CheckMods creates a local application data directory to store logs and ignored updates. On Windows, this defaults to `%AppData%/SptCheckMods/`.
- **Logs:** Rolling log files are stored at `%AppData%/SptCheckMods/logs/checkmod.log`.
- **Ignored Updates:** Your dismissed update alerts are stored in `%AppData%/SptCheckMods/ignored-updates.json`.

Internally, you can tweak the application settings by modifying configuration models:
- **`ForgeApiOptions`:** Override the default API URL (`https://forge.sp-tarkov.com/api/v0/`).
- **`RateLimitOptions`:** Adjust API burst limits and token refill rates.
- **`ModScannerOptions`:** Adjust file size limits for DLL scanning.

## Architecture (For Contributors)

This project leverages modern C# 13 features with strict nullability enabled. 
- **Core Stack:** Built entirely on standard `Microsoft.Extensions.*` libraries (Dependency Injection, Logging, Http).
- **Execution Flow:** `Program.cs` bootstraps the DI container, sets up a graceful cancellation token (listening for `Ctrl+C`), and passes control to `ApplicationService.cs`.
- **Build Process:** Uses custom MSBuild targets (`CheckMods.csproj`) to embed the active git commit hash into the executable and automatically rename/zip the published artifacts by Runtime Identifier (RID).
- **Testing:** The test suite (`Tests/CheckMods.Tests/`) uses xUnit. Instead of heavy mocking libraries, it relies on custom fakes (e.g., `FakeForgeApiService`) and temp workspace wrappers for robust, high-fidelity testing.

### Submitting a Pull Request
Please ensure all changes pass the xUnit test suite (`dotnet test`). Test files are strictly excluded from main compilations (`DefaultItemExcludes`).
