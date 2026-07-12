using CheckModsExtended.Models;
using OneOf;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CheckModsExtended.Services.Interfaces;

/// <summary>
/// Client for retrieving batch updates and dependency information from the Forge API.
/// </summary>
public interface IModUpdateClient
{
    Task<OneOf<ModUpdatesData, NotFound, ApiError>> GetModUpdatesAsync(IEnumerable<(int ModId, string CurrentVersion)> modUpdates, SemanticVersioning.Version sptVersion, CancellationToken cancellationToken = default);
    Task<OneOf<List<ModDependency>, NotFound, ApiError>> GetModDependenciesAsync(IEnumerable<(string Identifier, string Version)> modVersions, CancellationToken cancellationToken = default);
}
