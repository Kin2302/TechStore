$webAppPath = "d:\DotNet\TechStore\WebApp"
$appPath = "d:\DotNet\TechStore\Application"

$map = @{}

Write-Host "Reading Application types..."
$dtoFiles = Get-ChildItem -Path "$appPath\DTOs" -Recurse -Filter "*.cs"
$interfaceFiles = Get-ChildItem -Path "$appPath\Interfaces" -Recurse -Filter "*.cs"

foreach ($file in ($dtoFiles + $interfaceFiles)) {
    if ($file -eq $null) { continue }
    $category = $file.Directory.Name
    if ($category -eq "DTOs" -or $category -eq "Interfaces") { continue }

    $content = Get-Content $file.FullName -Raw
    $matches = [regex]::Matches($content, "(?:class|record|interface|struct|enum)\s+([A-Za-z0-9_]+)")
    foreach ($match in $matches) {
        $typeName = $match.Groups[1].Value
        
        if (-not $map.ContainsKey($category)) {
            $map[$category] = @()
        }
        if ($typeName -notin $map[$category]) {
            $map[$category] += $typeName
            Write-Host "Discovered: $typeName -> $category"
        }
    }
}

Write-Host "`nFixing WebApp files..."
$files = Get-ChildItem -Path $webAppPath -Recurse -Include *.cs, *.cshtml

foreach ($file in $files) {
    if ($file.FullName -match "\\obj\\") { continue }
    if ($file.FullName -match "\\bin\\") { continue }
    
    $content = Get-Content $file.FullName -Raw
    $newContent = $content
    
    foreach ($category in $map.Keys) {
        foreach ($typeName in $map[$category]) {
            $newContent = [regex]::Replace($newContent, "Application\.DTOs\.(?!$category\b)[\w\.]*\b$typeName\b", "Application.DTOs.$category.$typeName")
            $newContent = [regex]::Replace($newContent, "Application\.Interfaces\.(?!$category\b)[\w\.]*\b$typeName\b", "Application.Interfaces.$category.$typeName")
        }
    }

    if ($file.Extension -eq '.cs') {
        $newContent = $newContent -replace "(?m)^using Application\.DTOs;\r?\n?", ""
        $newContent = $newContent -replace "(?m)^using Application\.Interfaces;\r?\n?", ""
        
        $usingsToAdd = @(
            "using Application.DTOs.Admin;",
            "using Application.DTOs.Catalog;",
            "using Application.DTOs.Integration;",
            "using Application.DTOs.Orders;",
            "using Application.Interfaces.Admin;",
            "using Application.Interfaces.Catalog;",
            "using Application.Interfaces.Integration;",
            "using Application.Interfaces.Orders;"
        )
        
        $usingBlock = ""
        foreach ($u in $usingsToAdd) {
            if (-not $newContent.Contains($u)) {
                $usingBlock += "$u`r`n"
            }
        }
        
        if ($usingBlock -ne "") {
            if ($newContent -match "(?m)^using .*;") {
                $matches = [regex]::Matches($newContent, "(?m)^using .*;\r?\n")
                if ($matches.Count -gt 0) {
                    $lastMatch = $matches[$matches.Count - 1]
                    $insertPos = $lastMatch.Index + $lastMatch.Length
                    $newContent = $newContent.Insert($insertPos, $usingBlock)
                }
            } else {
                $newContent = $usingBlock + $newContent
            }
        }
    }
    
    if ($file.Name -eq '_ViewImports.cshtml') {
        $usingsToAdd = @(
            "@using Application.DTOs.Admin",
            "@using Application.DTOs.Catalog",
            "@using Application.DTOs.Integration",
            "@using Application.DTOs.Orders"
        )
        
        foreach ($u in $usingsToAdd) {
            if (-not $newContent.Contains($u)) {
                $newContent += "`r`n$u"
            }
        }
    }
    
    if ($content -ne $newContent) {
        Set-Content -Path $file.FullName -Value $newContent -NoNewline
        Write-Host "Fixed: $($file.FullName)"
    }
}
Write-Host "Done!"
