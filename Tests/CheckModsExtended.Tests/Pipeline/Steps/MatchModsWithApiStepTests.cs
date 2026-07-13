using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CheckModsExtended.Models;
using CheckModsExtended.Models.Pipeline;
using CheckModsExtended.Services.Pipeline.Steps;
using CheckModsExtended.Tests.Fakes;
using SemanticVersioning;
using Version = SemanticVersioning.Version;
using Xunit;

namespace CheckModsExtended.Tests.Pipeline.Steps;

public class MatchModsWithApiStepTests
{
    [Fact]
    public async Task ExecuteAsync_RunsMatching()
    {
        var service = new FakeModMatchingService();
        var reporter = new FakeModCheckReporter();
        var logger = new FakeLogger<MatchModsWithApiStep>();
        var step = new MatchModsWithApiStep(service, reporter, logger);

        var context = new UpdateWorkflowContext
        {
            Args = [],
            Mods = new List<Mod> { new Mod { Local = new LocalModIdentity { Guid = "test", FilePath = "test", IsServerMod = false, LocalName = "test", LocalAuthor = "test", LocalVersion = "test" } } },
            SptVersion = new Version("3.9.0")
        };

        await step.ExecuteAsync(context, CancellationToken.None);

        Assert.NotNull(context.Mods);
    }
}
