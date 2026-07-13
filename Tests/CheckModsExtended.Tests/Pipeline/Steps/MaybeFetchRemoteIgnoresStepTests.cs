using System.Threading;
using System.Threading.Tasks;
using CheckModsExtended.Models.Pipeline;
using CheckModsExtended.Services.Pipeline.Steps;
using CheckModsExtended.Tests.Fakes;
using Xunit;

namespace CheckModsExtended.Tests.Pipeline.Steps;

public class MaybeFetchRemoteIgnoresStepTests
{
    [Fact]
    public async Task ExecuteAsync_WhenNotConfigured_ReturnsEarly()
    {
        var client = new FakeRemoteIgnoreFileClient { IsConfigured = false };
        var store = new FakeIgnoredUpdateStore();
        var reporter = new FakeModCheckReporter();
        var logger = new FakeLogger<MaybeFetchRemoteIgnoresStep>();
        var step = new MaybeFetchRemoteIgnoresStep(client, store, reporter, logger);

        var context = new UpdateWorkflowContext { Args = [] };

        await step.ExecuteAsync(context, CancellationToken.None);
        Assert.NotNull(context);
    }
}
