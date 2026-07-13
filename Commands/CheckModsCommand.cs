using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CheckModsExtended.Models;
using CheckModsExtended.Services.Interfaces;
using Spectre.Console.Cli;

namespace CheckModsExtended.Commands;

/// <summary>
/// The main execution command for the application.
/// </summary>
public sealed class CheckModsCommand : AsyncCommand<CheckModsCommand.Settings>
{
    private readonly IUpdateWorkflowOrchestrator _orchestrator;
    private readonly IIgnoredUpdateWorkflow _ignoredUpdateWorkflow;
    private readonly IScanCacheService _scanCacheService;
    private readonly IUserPromptService _userPromptService;
    private readonly CheckModsExtended.Utils.IProcessRunner _processRunner;

    /// <summary>
    /// Command line settings.
    /// </summary>
    public sealed class Settings : GlobalSettings
    {
        [CommandArgument(0, "[SptPath]")]
        [Description("The path to your SPT installation directory. Defaults to the current directory.")]
        public string? SptPath { get; set; }
    }

    public CheckModsCommand(
        IUpdateWorkflowOrchestrator orchestrator, 
        IIgnoredUpdateWorkflow ignoredUpdateWorkflow,
        IScanCacheService scanCacheService,
        IUserPromptService userPromptService,
        CheckModsExtended.Utils.IProcessRunner processRunner)
    {
        _orchestrator = orchestrator;
        _ignoredUpdateWorkflow = ignoredUpdateWorkflow;
        _scanCacheService = scanCacheService;
        _userPromptService = userPromptService;
        _processRunner = processRunner;
    }

    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken
    )
    {
        var args = string.IsNullOrWhiteSpace(settings.SptPath) ? Array.Empty<string>() : new[] { settings.SptPath };

        var cache = await _scanCacheService.LoadCacheAsync(cancellationToken);
        if (cache != null && _userPromptService.PromptLoadFromCache(cache.CachedAtUtc))
        {
            var processPath = System.Environment.ProcessPath;
            if (processPath != null)
            {
                var guiArgs = string.IsNullOrWhiteSpace(settings.SptPath) ? "gui" : $"gui \"{settings.SptPath}\"";
                var startInfo = new System.Diagnostics.ProcessStartInfo(processPath, guiArgs)
                {
                    UseShellExecute = true
                };
                _processRunner.Start(startInfo);
                return 0;
            }
        }

        while (true)
        {
            var contextResult = await _orchestrator.RunPipelineAsync(args, cancellationToken);

            if (contextResult?.Mods is not null)
            {
                var endOfRunChoice = await _ignoredUpdateWorkflow.RunAsync(contextResult.Mods, cancellationToken);
                
                if (endOfRunChoice == EndOfRunChoice.Rescan)
                {
                    continue;
                }
                
                if (endOfRunChoice == EndOfRunChoice.LaunchWebGui)
                {
                    var processPath = System.Environment.ProcessPath;
                    if (processPath != null)
                    {
                        var guiArgs = string.IsNullOrWhiteSpace(settings.SptPath) ? "gui" : $"gui \"{settings.SptPath}\"";
                        var startInfo = new System.Diagnostics.ProcessStartInfo(processPath, guiArgs)
                        {
                            UseShellExecute = true
                        };
                        _processRunner.Start(startInfo);
                    }
                    break;
                }
            }

            break;
        }

        return 0; // Success
    }
}
