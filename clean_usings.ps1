$files = Get-ChildItem -Path d:\DotNet\TechStore\Application -Recurse -Filter *.cs
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $newContent = $content -replace "using Infrastructure\.Services\.(Admin|Catalog|Integration|Orders);\r?\n", ""
    if ($content -ne $newContent) {
        Set-Content -Path $file.FullName -Value $newContent -NoNewline
        Write-Host "Cleaned $file"
    }
}
