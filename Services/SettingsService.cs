using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CheckModsExtended.Models;
using CheckModsExtended.Services.Interfaces;
using CheckModsExtended.Services.Web;
using CheckModsExtended.Utils;
using SPTarkov.DI.Annotations;
using OneOf;

namespace CheckModsExtended.Services;

[Injectable(InjectionType.Transient)]
/// <summary>
/// Service for managing settings via the file system.
/// </summary>
public sealed class SettingsService : ISettingsService
{
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of the SettingsService class.
    /// </summary>
    /// <param name="fileSystem">The file system abstraction.</param>
    public SettingsService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// Gets the settings content as a JSON string.
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    public async Task<string> GetSettingsAsync(CancellationToken token = default)
    {
        string path = "appsettings.json";
        if (!_fileSystem.FileExists(path))
        {
            if (_fileSystem.FileExists("appsettings.example.json"))
            {
                return await _fileSystem.ReadAllTextAsync("appsettings.example.json", token);
            }
            return "{}";
        }
        return await _fileSystem.ReadAllTextAsync(path, token);
    }

    /// <summary>
    /// Updates the settings file with the provided JSON payload.
    /// </summary>
    /// <param name="jsonPayload">The JSON settings payload.</param>
    /// <param name="token">Cancellation token.</param>
    public async Task<OneOf<MessageResponse, ApiError>> UpdateSettingsAsync(string jsonPayload, CancellationToken token = default)
    {
        // Validate JSON before saving
        try { JsonDocument.Parse(jsonPayload); }
        catch { return new ApiError("Invalid JSON payload"); }

        await _fileSystem.WriteAllTextAsync("appsettings.json", jsonPayload, token);
        return new MessageResponse("Settings saved successfully. A restart may be required for some settings to take effect.");
    }
}
