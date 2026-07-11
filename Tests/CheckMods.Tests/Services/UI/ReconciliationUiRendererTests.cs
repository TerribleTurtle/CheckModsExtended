using System.Collections.Generic;
using CheckMods.Models;
using CheckMods.Services.Interfaces;
using CheckMods.Services.UI;
using CheckMods.Tests.Fakes;
using Spectre.Console;
using Spectre.Console.Testing;
using Xunit;

using CheckMods.Tests.Fixtures;

namespace CheckMods.Tests.Services.UI;

[Collection("ConsoleTests")]
public sealed class ReconciliationUiRendererTests
{
    [Fact]
    public void Unverified_mods_renders_list()
    {
        var console = new TestConsole();
        AnsiConsole.Console = console;
        var renderer = new ReconciliationUiRenderer(new FakeTextRenderer());

        var mod = ModFixture.CreateClientMod("unverified.mod", "Unverified Mod");
        mod = mod.MarkUnmatched();

        renderer.UnverifiedMods(new List<Mod> { mod });

        var output = console.Output;
        Assert.Contains("Unverified Mod", output);
        Assert.Contains("unverified.mod", output);
    }

    [Fact]
    public void Loading_warnings_renders_list()
    {
        var console = new TestConsole();
        AnsiConsole.Console = console;
        var renderer = new ReconciliationUiRenderer(new FakeTextRenderer());

        var mod = new Mod
        {
            Local = new LocalModIdentity
            {
                Guid = "warning.mod",
                FilePath = "warning.dll",
                IsServerMod = false,
                LocalName = "Warning Mod",
                LocalAuthor = "Author",
                LocalVersion = "1.0.0",
            },
            LoadWarnings = new List<string> { "Mod load failed" },
        };

        renderer.LoadingWarnings(new List<Mod> { mod });

        var output = console.Output;
        Assert.Contains("Mod loading warnings", output);
        Assert.Contains("Warning Mod", output);
        Assert.Contains("Mod load failed", output);
    }

    [Fact]
    public void Reconciliation_results_renders_orphans()
    {
        var console = new TestConsole();
        AnsiConsole.Console = console;
        var renderer = new ReconciliationUiRenderer(new FakeTextRenderer());

        var clientMod = ModFixture.CreateClientMod("client.mod", "Client Mod");

        var serverMod = ModFixture.CreateServerMod("server.mod", "Server Mod");

        var result = new ModReconciliationResult
        {
            Mods = new List<Mod>(),
            ReconciledPairs = new List<ModPair>(),
            UnmatchedClientMods = new List<Mod> { clientMod },
            UnmatchedServerMods = new List<Mod> { serverMod },
        };

        renderer.ReconciliationResults(result);

        var output = console.Output;
        Assert.Contains("Final mod count", output);
        Assert.Contains("server-only: 1", output);
        Assert.Contains("client-only: 1", output);
    }

    [Fact]
    public void Reconciliation_results_renders_reconciled_pairs_with_notes_and_guid_mismatches()
    {
        var console = new TestConsole();
        AnsiConsole.Console = console;
        var renderer = new ReconciliationUiRenderer(new FakeTextRenderer());

        var serverMod = new Mod
        {
            Local = new LocalModIdentity
            {
                Guid = "server.guid",
                IsServerMod = true,
                LocalName = "Test Mod",
                FilePath = "server.dll",
                LocalAuthor = "Author",
                LocalVersion = "1.0.0",
            },
            Api = new ForgeApiMetadata { ApiUrl = "https://forge.com/mod" },
        };

        var clientMod = ModFixture.CreateClientMod("client.guid", "Test Mod");

        var pair = new ModPair
        {
            ServerMod = serverMod,
            ClientMod = clientMod,
            SelectedMod = serverMod,
            Notes = new List<string> { "This is a warning note" },
        };

        var result = new ModReconciliationResult
        {
            Mods = new List<Mod> { serverMod },
            ReconciledPairs = new List<ModPair> { pair },
            UnmatchedServerMods = new List<Mod>(),
            UnmatchedClientMods = new List<Mod>(),
        };

        renderer.ReconciliationResults(result);

        var output = console.Output;
        Assert.Contains("This is a warning note", output);
        Assert.Contains("GUIDs differ", output);
    }
}
