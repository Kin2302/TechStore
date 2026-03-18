$usings = @"
using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;
using Infrastructure.Services.Admin;
using Infrastructure.Services.Catalog;
using Infrastructure.Services.Integration;
using Infrastructure.Services.Orders;
"@

$content = Get-Content "d:\DotNet\TechStore\build_errors.txt" -Raw
$matches = [regex]::Matches($content, "(?m)^([A-Z]:\\[^\(]+.cs)\(")
$files = $matches | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique

foreach ($file in $files) {
    if (Test-Path $file) {
        $fileContent = Get-Content $file -Raw
        if (-not $fileContent.Contains("using Application.DTOs.Catalog;")) {
            $newContent = $usings + "`r`n" + $fileContent
            Set-Content -Path $file -Value $newContent -NoNewline
            Write-Host "Fixed: $file"
        }
    }
}
