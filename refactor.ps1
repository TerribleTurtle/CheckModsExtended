$files = Get-ChildItem -Path "Tests\CheckMods.Tests" -Recurse -Filter "*.cs"
foreach ($file in $files) {
    if ($file.Name -eq "ModFixture.cs") { continue }
    $content = Get-Content -Raw $file.FullName
    
    # Simple state machine to replace new Mod { Local = new LocalModIdentity { ... } };
    
    $pattern = 'new Mod\s*\{\s*Local\s*=\s*new LocalModIdentity\s*\{([^\}]+)\}(?:,\s*)?\s*\}'
    
    $matches = [regex]::Matches($content, $pattern)
    if ($matches.Count -eq 0) { continue }
    
    foreach ($m in $matches) {
        $block = $m.Groups[1].Value
        $isServer = $block -match 'IsServerMod\s*=\s*true'
        $guidMatch = [regex]::Match($block, 'Guid\s*=\s*"([^"]+)"')
        $guid = if ($guidMatch.Success) { $guidMatch.Groups[1].Value } else { "test" }
        
        $nameMatch = [regex]::Match($block, 'LocalName\s*=\s*"([^"]+)"')
        $name = if ($nameMatch.Success) { $nameMatch.Groups[1].Value } else { "Mod" }
        
        $versionMatch = [regex]::Match($block, 'LocalVersion\s*=\s*"([^"]+)"')
        $version = if ($versionMatch.Success) { $versionMatch.Groups[1].Value } else { "1.0.0" }
        
        $authorMatch = [regex]::Match($block, 'LocalAuthor\s*=\s*"([^"]+)"')
        $author = if ($authorMatch.Success) { $authorMatch.Groups[1].Value } else { "Author" }
        
        $args = "`"$guid`""
        if ($name -ne "Mod" -or $version -ne "1.0.0" -or $author -ne "Author") { $args += ", `"$name`"" }
        if ($version -ne "1.0.0" -or $author -ne "Author") { $args += ", `"$version`"" }
        if ($author -ne "Author") { $args += ", `"$author`"" }
        
        $method = if ($isServer) { "CreateServerMod" } else { "CreateClientMod" }
        $replacement = "ModFixture.$method($args)"
        
        $content = $content.Replace($m.Value, $replacement)
    }
    
    # We might need to add `using CheckMods.Tests.Fixtures;`
    if ($content -notmatch "using CheckMods.Tests.Fixtures;") {
        $content = $content -replace "(using [^;]+;`r?`n)(namespace)", "`$1using CheckMods.Tests.Fixtures;`r`n`n`$2"
    }
    
    Set-Content -Path $file.FullName -Value $content -NoNewline
}
