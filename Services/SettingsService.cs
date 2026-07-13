using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CheckModsExtended.Models;
using CheckModsExtended.Services.Interfaces;
using CheckModsExtended.Utils;
using SPTarkov.DI.Annotations;
using OneOf;

namespace CheckModsExtended.Services;

[Injectable(InjectionType.Transient)]
public class SettingsService : ISettingsService
{
    private readonly IFileSystem _fileSystem;

    public SettingsService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public async Task<string> GetSettingsAsync(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<OneOf<MessageResponse, ApiError>> UpdateSettingsAsync(string jsonPayload, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}
