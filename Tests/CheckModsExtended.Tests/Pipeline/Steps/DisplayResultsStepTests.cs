using System.Threading;
using System.Threading.Tasks;
using CheckModsExtended.Configuration;
using CheckModsExtended.Models.Pipeline;
using CheckModsExtended.Services.Pipeline.Steps;
using CheckModsExtended.Tests.Fakes;
using Xunit;

namespace CheckModsExtended.Tests.Pipeline.Steps;

public class DisplayResultsStepTests
{
    [Fact]
    public async Task ExecuteAsync_CompletesSuccessfully()
    {
        var reporter = new FakeModCheckReporter();
        var logger = new FakeLogger<DisplayResultsStep>();
        var config = new RuntimeConfig();
        var step = new DisplayResultsStep(reporter, logger, config);

        var context = new UpdateWorkflowContext { Args = [] };

        await step.ExecuteAsync(context, CancellationToken.None);

        Assert.Contains(logger.LoggedMessages, m => m.Contains("completed successfully"));
    }
}
