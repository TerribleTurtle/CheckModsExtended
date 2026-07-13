using System.Threading;
using System.Threading.Tasks;
using CheckModsExtended.Models.Pipeline;
using CheckModsExtended.Services.Pipeline.Steps;
using CheckModsExtended.Tests.Fakes;
using Xunit;

namespace CheckModsExtended.Tests.Pipeline.Steps;

public class RemoveLegacyApiKeyStepTests
{
    [Fact]
    public async Task ExecuteAsync_CallsRemove()
    {
        var service = new FakeInitializationService();
        var step = new RemoveLegacyApiKeyStep(service);

        var context = new UpdateWorkflowContext { Args = [] };

        await step.ExecuteAsync(context, CancellationToken.None);

        Assert.True(service.RemoveLegacyApiKeyFileCalled);
    }
}
