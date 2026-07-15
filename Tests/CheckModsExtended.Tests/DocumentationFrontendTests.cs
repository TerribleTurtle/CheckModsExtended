using System.IO;
using Xunit;

namespace CheckModsExtended.Tests;

public class DocumentationFrontendTests
{
    private string GetWwwRootPath()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null)
        {
            var wwwroot = Path.Combine(current.FullName, "wwwroot");
            if (Directory.Exists(wwwroot))
            {
                return wwwroot;
            }
            current = current.Parent;
        }
        throw new DirectoryNotFoundException("Could not find wwwroot");
    }

    [Fact]
    public void IndexHtml_ShouldNotHaveDuplicateScript()
    {
        var path = Path.Combine(GetWwwRootPath(), "index.html");
        var content = File.ReadAllText(path);
        Assert.DoesNotContain("<script type=\"module\" src=\"js/main.js\"></script>", content);
    }

    [Fact]
    public void ApiJs_ShouldHaveSignalParamDoc()
    {
        var path = Path.Combine(GetWwwRootPath(), "js", "api.js");
        var content = File.ReadAllText(path);
        Assert.Contains("@param {AbortSignal} signal", content);
    }

    [Fact]
    public void MainJs_ShouldHaveSchemaJSDoc()
    {
        var path = Path.Combine(GetWwwRootPath(), "js", "main.js");
        var content = File.ReadAllText(path);
        
        Assert.Contains("@typedef {Object} ModManagerData", content);
    }

    [Fact]
    public void MainJs_ShouldHaveWhyComments()
    {
        var path = Path.Combine(GetWwwRootPath(), "js", "main.js");
        var content = File.ReadAllText(path);
        
        Assert.Contains("Why: We filter and sort the full mods array locally", content);
        Assert.Contains("Why: Runs a lightweight hash check to see if local mods changed", content);
        Assert.Contains("Why: Enables keyboard navigation (Vim-style j/k or arrows)", content);
    }
}
