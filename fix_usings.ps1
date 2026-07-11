$files = Get-ChildItem -Path "Tests\CheckMods.Tests" -Recurse -Filter "*.cs"
foreach ($file in $files) {
    $content = Get-Content -Raw $file.FullName
    if ($content -match "ModFixture" -and $content -notmatch "using CheckMods.Tests.Fixtures;") {
        $content = $content -replace "(?m)^(namespace .*)", "using CheckMods.Tests.Fixtures;`r`n`r`n`$1"
        Set-Content -Path $file.FullName -Value $content -NoNewline
    }
}
