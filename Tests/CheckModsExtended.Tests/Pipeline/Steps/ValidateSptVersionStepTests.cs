using System.Threading;
using System.Threading.Tasks;
using CheckModsExtended.Models.Pipeline;
using CheckModsExtended.Services.Pipeline.Steps;
using CheckModsExtended.Tests.Fakes;
using SemanticVersioning;
using Version = SemanticVersioning.Version;
using Xunit;

namespace CheckModsExtended.Tests.Pipeline.Steps;

public class ValidateSptVersionStepTests
{
    [Fact]
    public async Task ExecuteAsync_WhenVersionIsNull_CancelsContext()
    {
        var installationService = new FakeSptInstallationService();
        var orchestrationService = new FakeUpdateOrchestrationService();
        var reporter = new FakeModCheckReporter();
        var logger = new FakeLogger<ValidateSptVersionStep>();

        var step = new ValidateSptVersionStep(installationService, orchestrationService, reporter, logger);

        var context = new UpdateWorkflowContext
        {
            Args = [],
            SptPath = "C:\\SPT"
        };

        await step.ExecuteAsync(context, CancellationToken.None);

        Assert.True(context.IsCancelled);
    }

    [Fact]
    public async Task ExecuteAsync_WhenVersionIsValid_SetsVersion()
    {
        var installationService = new FakeSptInstallationService();
        installationService.ValidatedVersion = new Version("3.9.0");
        var orchestrationService = new FakeUpdateOrchestrationService();
        var reporter = new FakeModCheckReporter();
        var logger = new FakeLogger<ValidateSptVersionStep>();

        var step = new ValidateSptVersionStep(installationService, orchestrationService, reporter, logger);

        var context = new UpdateWorkflowContext
        {
            Args = [],
            SptPath = "C:\\SPT"
        };

        await step.ExecuteAsync(context, CancellationToken.None);

        Assert.False(context.IsCancelled);
        Assert.Equal(new Version("3.9.0"), context.SptVersion);
    }
}
