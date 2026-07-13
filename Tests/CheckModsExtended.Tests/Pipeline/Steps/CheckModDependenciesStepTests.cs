using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CheckModsExtended.Models;
using CheckModsExtended.Models.Pipeline;
using CheckModsExtended.Services.Pipeline.Steps;
using CheckModsExtended.Tests.Fakes;
using Xunit;

namespace CheckModsExtended.Tests.Pipeline.Steps;

public class CheckModDependenciesStepTests
{
    [Fact]
    public async Task ExecuteAsync_NoMatchedMods_ReturnsEarly()
    {
        var dependencyService = new FakeModDependencyService();
        var reporter = new FakeModCheckReporter();
        var logger = new FakeLogger<CheckModDependenciesStep>();
        var step = new CheckModDependenciesStep(dependencyService, reporter, logger);

        var context = new UpdateWorkflowContext
        {
            Args = [],
            Mods = new List<Mod> { new Mod { Status = ModStatus.NoMatch, Local = new LocalModIdentity { Guid = "test", FilePath = "test", IsServerMod = false, LocalName = "test", LocalAuthor = "test", LocalVersion = "test" } } }
        };

        await step.ExecuteAsync(context, CancellationToken.None);

        Assert.Empty(reporter.Headings);
    }
}
