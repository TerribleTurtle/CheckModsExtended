using System.Reflection;
using CheckMods.Services;
using CheckMods.Tests.Fixtures;
using SemanticVersioning;
using Xunit;
using Version = SemanticVersioning.Version;

namespace CheckMods.Tests.Utils;

/// <summary>
/// Tests for <see cref="AssemblyResolver"/>.
/// </summary>
public sealed class AssemblyResolverTests : IDisposable
{
    private readonly string _workspace;

    public AssemblyResolverTests()
    {
        _workspace = TempWorkspace.CreateDirectory("AssemblyResolver");
    }

    public void Dispose()
    {
        TempWorkspace.SafeDelete(_workspace);
    }

    [Fact]
    public void Resolve_locates_bepinex_core_assemblies_and_returns_null_for_missing()
    {
        // Arrange
        var targetDll = Path.Combine(_workspace, "target.dll");
        File.Copy(typeof(AssemblyResolver).Assembly.Location, targetDll);

        var coreDir = Path.Combine(_workspace, "BepInEx", "core");
        Directory.CreateDirectory(coreDir);
        var dummyDll = Path.Combine(coreDir, "SemanticVersioning.dll");
        File.Copy(typeof(Version).Assembly.Location, dummyDll);

        var sut = new AssemblyResolver(targetDll);
        using var context = new MetadataLoadContext(sut);

        // Act & Assert
        // Verify it can locate the assembly in BepInEx/core
        var resolvedAssembly = sut.Resolve(context, new AssemblyName("SemanticVersioning"));
        Assert.NotNull(resolvedAssembly);
        Assert.Equal("SemanticVersioning", resolvedAssembly.GetName().Name);

        // Verify it returns null for a missing assembly instead of throwing
        var missingAssembly = sut.Resolve(context, new AssemblyName("NonExistentAssembly"));
        Assert.Null(missingAssembly);
    }
}
