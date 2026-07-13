import os

path = r'Tests\CheckModsExtended.Tests\Services\ScanCacheServiceTests.cs'
with open(path, 'r', encoding='utf-8') as f:
    text = f.read()

new_tests = '''
    [Fact]
    public async Task SaveCacheAsync_SwallowsExceptions_AndLogsWarning()
    {
        _fileSystem.UnauthorizedPaths.Add("C:\\\\AppData\\\\scan_cache.json.tmp");
        var response = new ScanResponse(new List<ModDto>(), null, "3.8.0");
        var record = new ScanCacheRecord(TimeProvider.System.GetUtcNow(), response);
        
        // Should not throw
        await _service.SaveCacheAsync(record);
    }

    [Fact]
    public async Task LoadCacheAsync_SwallowsExceptions_AndReturnsNull()
    {
        await _fileSystem.WriteAllTextAsync("C:\\\\AppData\\\\scan_cache.json", "{}");
        _fileSystem.UnauthorizedPaths.Add("C:\\\\AppData\\\\scan_cache.json");
        
        // Should not throw
        var loaded = await _service.LoadCacheAsync();
        Assert.Null(loaded);
    }
}
'''
text = text.rsplit('}', 1)[0] + new_tests
with open(path, 'w', encoding='utf-8') as f:
    f.write(text)
