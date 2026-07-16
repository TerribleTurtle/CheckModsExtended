using CheckModsExtended.Services.Interfaces;
using CheckModsExtended.Utils;
using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;

namespace CheckModsExtended.Services;

/// <summary>
/// Implementation of <see cref="IInitializationService"/>.
/// </summary>
[Injectable(InjectionType.Transient)]
public sealed class InitializationService(
    IModCheckReporter reporter,
    ILogger<InitializationService> logger,
    Microsoft.Extensions.Options.IOptions<CheckModsExtended.Configuration.AppPaths> appPaths,
    IFileSystem fileSystem
) : IInitializationService
{
    /// <inheritdoc />
    public async Task RemoveLegacyApiKeyFileAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(appPaths.Value.AppDataDirectory))
            {
                return;
            }

            var configDirectory = Path.GetFullPath(appPaths.Value.AppDataDirectory);
            var configFilePath = Path.GetFullPath(Path.Combine(configDirectory, "apikey.txt"));

            if (!fileSystem.FileExists(configFilePath))
            {
                return;
            }

            await Task.Run(() => fileSystem.DeleteFile(configFilePath));
            logger.LogInformation("Removed legacy API key file.");
        }
        catch (IOException ex)
        {
            logger.LogWarning(ex, "Failed to remove legacy API key file due to IO error");
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Failed to remove legacy API key file due to access permissions");
        }
    }

    /// <inheritdoc />
    public string? GetValidatedSptPath(string[] args)
    {
        reporter.Banner();
        reporter.Heading("Validating SPT installation...");

        string? basePath;
        if (args.Length == 0)
        {
            basePath = fileSystem.GetCurrentDirectory();
        }
        else
        {
            basePath = SecurityHelper.GetSafePath(args[0]);
            if (basePath is null)
            {
                reporter.Error("Error: Invalid path provided.");
                return null;
            }
        }

        if (!fileSystem.DirectoryExists(basePath))
        {
            reporter.DirectoryDoesNotExist(basePath);
            return null;
        }

        var resolvedPath = ResolveNestedSptPath(basePath);

        reporter.UsingPath(resolvedPath);
        return resolvedPath;
    }

    private string ResolveNestedSptPath(string basePath)
    {
        var possibleExecutables = new[] { "SPT.Server.exe", "SPTarkov.Server.Core.dll", "Aki.Server.exe" };
        
        // First check the base path
        foreach (var exe in possibleExecutables)
        {
            if (fileSystem.FileExists(Path.Combine(basePath, exe)))
            {
                return basePath;
            }
        }

        // If not found, check immediate subdirectories (depth 1)
        try
        {
            var subDirs = fileSystem.GetDirectories(basePath);
            foreach (var dir in subDirs)
            {
                foreach (var exe in possibleExecutables)
                {
                    if (fileSystem.FileExists(Path.Combine(dir, exe)))
                    {
                        reporter.Warning($"Auto-detected nested SPT folder: {Path.GetFileName(dir)}");
                        return dir;
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            logger.LogWarning(ex, "Failed to inspect subdirectories for nested SPT installation");
        }

        return basePath;
    }
}
