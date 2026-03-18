$map = @{
    'Catalog' = @('BrandDto', 'CategoryDto', 'ProductDto', 'ProductCreateDto', 'ProductEditDto', 'ProductDetailDto', 'SpecificationInputDto', 'ReviewDto', 'CreateReviewDto', 'CompareItemDto', 'IProductService', 'IReviewService', 'ICompareService')
    'Orders' = @('CartItemDto', 'CheckoutDto', 'OrderDto', 'CreateOrderResult', 'ICartService', 'IOrderService')
    'Admin' = @('DashboardDto', 'IAdminBrandService', 'IAdminCategoryService', 'IAdminOrderService', 'IAdminProductService', 'IDashboardService')
    'Integration' = @('MoMoOptions', 'MoMoCreatePaymentResultDto', 'ChatDto', 'AnalysisResultDto', 'ProjectSuggestionDto', 'IMoMoService', 'IGeminiService')
}

$files = Get-ChildItem -Path d:\DotNet\TechStore\WebApp -Recurse -Include *.cs, *.cshtml

foreach ($file in $files) {
    if ($file.FullName -match "obj\\") { continue }
    if ($file.FullName -match "bin\\") { continue }
    
    $content = Get-Content $file.FullName -Raw
    $newContent = $content
    
    foreach ($category in $map.Keys) {
        foreach ($type in $map[$category]) {
            # Fix @model Application.DTOs.BrandDto
            $newContent = $newContent -replace "Application\.DTOs\.$type", "Application.DTOs.$category.$type"
            $newContent = $newContent -replace "Application\.Interfaces\.$type", "Application.Interfaces.$category.$type"
        }
    }
    
    if ($file.Extension -eq '.cs') {
        $newContent = $newContent -replace 'using Application\.DTOs;', "using Application.DTOs.Admin;`r`nusing Application.DTOs.Catalog;`r`nusing Application.DTOs.Integration;`r`nusing Application.DTOs.Orders;"
        $newContent = $newContent -replace 'using Application\.Interfaces;', "using Application.Interfaces.Admin;`r`nusing Application.Interfaces.Catalog;`r`nusing Application.Interfaces.Integration;`r`nusing Application.Interfaces.Orders;"
    }
    
    if ($file.Name -eq '_ViewImports.cshtml') {
        if (-not $newContent.Contains("using Application.DTOs.Catalog")) {
            $newContent = $newContent + "`r`n@using Application.DTOs.Admin`r`n@using Application.DTOs.Catalog`r`n@using Application.DTOs.Integration`r`n@using Application.DTOs.Orders"
        }
    }
    
    if ($content -ne $newContent) {
        Set-Content -Path $file.FullName -Value $newContent -NoNewline
        Write-Host "Fixed: $($file.Name)"
    }
}
