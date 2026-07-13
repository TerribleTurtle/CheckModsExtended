using System.Text.Json;
using System.Threading.Tasks;
using CheckModsExtended.Models;
using CheckModsExtended.Services;
using CheckModsExtended.Services.Web;
using CheckModsExtended.Tests.Fixtures;
using Xunit;

namespace CheckModsExtended.Tests.Services;

public class SettingsServiceTests
{
    private readonly FakeFileSystem _fileSystem;
    private readonly SettingsService _settingsService;

    public SettingsServiceTests()
    {
        _fileSystem = new FakeFileSystem();
        _settingsService = new SettingsService(_fileSystem);
    }

    [Fact]
    public async Task GetSettingsAsync_FallsBackToExample_IfAppSettingsIsMissing()
    {
        var exampleJson = "{\"setting\": \"example\"}";
        _fileSystem.Files["appsettings.example.json"] = System.Text.Encoding.UTF8.GetBytes(exampleJson);

        var result = await _settingsService.GetSettingsAsync();

        Assert.Equal(exampleJson, result);
    }

    [Fact]
    public async Task GetSettingsAsync_ReturnsEmptyObject_IfBothAreMissing()
    {
        var result = await _settingsService.GetSettingsAsync();

        Assert.Equal("{}", result);
    }

    [Fact]
    public async Task UpdateSettingsAsync_ReturnsSuccess_IfJsonIsValid()
    {
        var validJson = "{\"setting\": \"new value\"}";

        var result = await _settingsService.UpdateSettingsAsync(validJson);

        Assert.True(result.IsT0);
        Assert.NotNull(result.AsT0.Message);
        
        var writtenContent = await _fileSystem.ReadAllTextAsync("appsettings.json");
        Assert.Equal(validJson, writtenContent);
    }

    [Fact]
    public async Task UpdateSettingsAsync_ReturnsApiError_IfJsonIsInvalid()
    {
        var invalidJson = "{ invalid json";

        var result = await _settingsService.UpdateSettingsAsync(invalidJson);

        Assert.True(result.IsT1);
        Assert.IsType<ApiError>(result.AsT1);
        Assert.False(_fileSystem.FileExists("appsettings.json"));
    }
}
