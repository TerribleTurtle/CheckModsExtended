using CheckModsExtended.Models;
using OneOf;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CheckModsExtended.Services.Interfaces;

/// <summary>
/// Client for interacting with SPT version information from the Forge API.
/// </summary>
public interface ISptVersionClient
{
    Task<OneOf<bool, InvalidSptVersion, ApiError>> ValidateSptVersionAsync(string sptVersion, CancellationToken cancellationToken = default);
    Task<OneOf<List<SptVersionResult>, ApiError>> GetAllSptVersionsAsync(CancellationToken cancellationToken = default);
}
