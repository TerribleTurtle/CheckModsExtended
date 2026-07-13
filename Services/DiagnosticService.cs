using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using CheckModsExtended.Configuration;
using CheckModsExtended.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace CheckModsExtended.Services;

public class DiagnosticService : IDiagnosticService
{
    private readonly AppPaths _appPaths;

    public DiagnosticService(IOptions<AppPaths> appPaths)
    {
        _appPaths = appPaths.Value;
    }

    public Task<string?> ExportLogsAsync(CancellationToken cancellationToken = default)
    {
        var logsDir = Path.Combine(_appPaths.AppDataDirectory, "logs");
        if (!Directory.Exists(logsDir))
        {
            return Task.FromResult<string?>(null);
        }

        var zipPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            $"spt-check-mods-logs-{DateTime.Now:yyyyMMdd-HHmmss}.zip"
        );
        ZipFile.CreateFromDirectory(logsDir, zipPath);
        return Task.FromResult<string?>(zipPath);
    }
}
