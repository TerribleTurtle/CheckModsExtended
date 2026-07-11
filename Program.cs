using CheckModsExtended.Commands;
using CheckModsExtended.Configuration;
using CheckModsExtended.Extensions;
using CheckModsExtended.Models;
using CheckModsExtended.Services.Interfaces;
using CheckModsExtended.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

namespace CheckModsExtended;

/// <summary>
/// Main entry point for the CheckModsExtended application.
/// </summary>
public sealed class Program
{
    private static CancellationTokenSource? _cts;
    private static bool _wasCancelled;

    /// <summary>
    /// Exposes the global cancellation token for commands.
    /// </summary>
    public static CancellationToken CancellationToken
    {
        get { return _cts?.Token ?? CancellationToken.None; }
    }

    /// <summary>
    /// Sets up dependency injection, runs the application, and handles any unhandled exceptions.
    /// </summary>
    /// <param name="args">Command line arguments. The only argument is the SPT installation path.</param>
    public static async Task<int> Main(string[] args)
    {
        int exitCode = 0;
        ILogger<Program>? logger = null;
        ServiceProvider? serviceProvider = null;
        string? logFilePath = null;

        _wasCancelled = false;
        _cts = new CancellationTokenSource();
        Console.CancelKeyPress += OnCancelKeyPress;

        WindowsConsoleHelper.TryEnableVirtualTerminalProcessing();

        if (!Console.IsOutputRedirected)
        {
            AnsiConsole.Profile.Capabilities.Ansi = true;
            AnsiConsole.Profile.Capabilities.ColorSystem = ColorSystem.Standard;
        }

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var services = new ServiceCollection();
            services.AddCheckModsExtendedServices(configuration);
            
            var registrar = new TypeRegistrar(services);
            var app = new CommandApp<CheckModsCommand>(registrar);

            app.Configure(config =>
            {
                config.SetApplicationName("check-mods");
                config.SetApplicationVersion(VersionInfo.SemVer);
                config.PropagateExceptions(); // Let the try-catch block handle exceptions
            });

            // We must build the service provider manually to get the logger and log file path for the footer
            serviceProvider = services.BuildServiceProvider();
            logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            logFilePath = serviceProvider.GetRequiredService<IOptions<LoggingOptions>>().Value.LogFilePath;

            logger.LogInformation("CheckModsExtended application starting. Args: {Args}", string.Join(", ", args));

            exitCode = await app.RunAsync(args);

            logger.LogInformation("CheckModsExtended application completed with exit code {ExitCode}", exitCode);
        }
        catch (OperationCanceledException)
        {
            logger?.LogInformation("Application was cancelled by user");
            exitCode = 0; // Usually cancelled is a zero exit code
        }
        catch (Exception ex)
        {
            logger?.LogCritical(ex, "Unhandled exception occurred");
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);
            exitCode = 2;
        }
        finally
        {
            Console.CancelKeyPress -= OnCancelKeyPress;
            _cts.Dispose();
            _cts = null;

            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine($"[grey]Check Mods v{VersionInfo.SemVer} (build {VersionInfo.GitHash})[/]");
            if (logFilePath != null)
            {
                AnsiConsole.MarkupLine($"[grey]Log file: {logFilePath}[/]");
            }

            var isHeadless = Console.IsInputRedirected || args.Contains("--no-prompt") || args.Contains("-y");

            if (!_wasCancelled && !isHeadless)
            {
                while (Console.KeyAvailable)
                {
                    Console.ReadKey(intercept: true);
                }

                AnsiConsole.MarkupLine("[grey]Press any key to exit...[/]");
                Console.ReadKey();
            }

            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }
        }

        return exitCode;
    }

    private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        _wasCancelled = true;
        _cts?.Cancel();
        AnsiConsole.MarkupLine("[yellow]Cancellation requested. Shutting down gracefully...[/]");
    }
}

