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

public class CheckModVersionCompatibilityStepTests
{
    [Fact]
    public async Task ExecuteAsync_CallsValidationService()
    {
        var validationService = new FakeCompatibilityValidationService();
        var reporter = new FakeModCheckReporter();
        var logger = new FakeLogger<CheckModVersionCompatibilityStep>();
        var step = new CheckModVersionCompatibilityStep(validationService, reporter, logger);

        var context = new UpdateWorkflowContext
        {
            Args = [],
            Mods = new List<Mod> { new Mod { Local = new LocalModIdentity { Guid = "test", FilePath = "test", IsServerMod = false, LocalName = "test", LocalAuthor = "test", LocalVersion = "test" } } },
            SptVersion = new Version("3.9.0")
        };

        await step.ExecuteAsync(context, CancellationToken.None);

        Assert.True(validationService.CheckModVersionCompatibilityCalled);
    }
}
