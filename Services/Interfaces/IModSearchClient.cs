using CheckModsExtended.Models;
using OneOf;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CheckModsExtended.Services.Interfaces;

/// <summary>
/// Client for searching and retrieving individual mods from the Forge API.
/// </summary>
public interface IModSearchClient
{
    Task<OneOf<List<ModSearchResult>, ApiError>> SearchModsAsync(string modName, SemanticVersioning.Version sptVersion, CancellationToken cancellationToken = default);
    Task<OneOf<List<ModSearchResult>, ApiError>> SearchClientModsAsync(string modName, SemanticVersioning.Version sptVersion, CancellationToken cancellationToken = default);
    Task<OneOf<ModSearchResult, NotFound, InvalidInput, ApiError>> GetModByIdAsync(int modId, CancellationToken cancellationToken = default);
    Task<OneOf<ModSearchResult, NotFound, NoCompatibleVersion, ApiError>> GetModByGuidAsync(string modGuid, SemanticVersioning.Version sptVersion, CancellationToken cancellationToken = default);
}
