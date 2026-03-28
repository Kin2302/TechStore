using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using System.ComponentModel;
using System.Text.Json;

namespace TechStore.Infrastructure.Plugins
{
    /// <summary>
    /// Plugin cho AI Agent de tim kiem va lay thong tin san pham
    /// </summary>
    public class ProductPlugin
    {
        private readonly IProductService _productService;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);

        public ProductPlugin(IProductService productService, IConfiguration configuration)
        {
            _productService = productService;
            _configuration = configuration;
        }

        [KernelFunction("search_products")]
        [Description("Tim kiem san pham theo tu khoa, gia, danh muc. Tra ve danh sach san pham phu hop.")]
        public async Task<string> SearchProductsAsync(
            [Description("Tu khoa tim kiem (vi du: 'Arduino', 'ESP32', 'cam bien nhiet do')")]
            string keyword,

            [Description("Gia toi da (VND). De trong neu khong gioi han")]
            decimal? maxPrice = null,

            [Description("So luong ket qua toi da")]
            int limit = 5)
        {
            var products = await _productService.SearchProductsAsync(keyword, maxPrice, limit);

            if (!products.Any())
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    message = $"Khong tim thay san pham nao voi tu khoa '{keyword}'",
                    count = 0,
                    products = Array.Empty<object>()
                });

            var result = products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                price = p.Price,
                stock = p.Stock,
                category = p.CategoryName,
                brand = p.BrandName
            }).ToList();

            return JsonSerializer.Serialize(new
            {
                success = true,
                count = result.Count,
                message = $"Tim thay {result.Count} san pham",
                products = result
            });
        }

        [KernelFunction("get_product_details")]
        [Description("Lay thong tin chi tiet cua mot san pham cu the theo ID")]
        public async Task<string> GetProductDetailsAsync(
            [Description("ID cua san pham can xem chi tiet")]
            int productId)
        {
            var product = await _productService.GetByIdAsync(productId);

            if (product == null)
                return $"Khong tim thay san pham voi ID {productId}";

            return JsonSerializer.Serialize(new
            {
                success = true,
                product = new
                {
                    id = product.Id,
                    name = product.Name,
                    code = product.Code,
                    price = product.Price,
                    stock = product.Stock,
                    description = product.ShortDescription,
                    category = product.CategoryName,
                    brand = product.BrandName,
                    specifications = product.Specifications.Select(s => new
                    {
                        name = s.Name,
                        value = s.Value
                    })
                }
            });
        }

        [KernelFunction("get_popular_products")]
        [Description("Lay danh sach san pham ban chay nhat")]
        public async Task<string> GetPopularProductsAsync(
            [Description("So luong san pham can lay")]
            int count = 5)
        {
            var products = await _productService.GetFeaturedAsync(count);

            var result = products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                price = p.Price,
                stock = p.Stock
            }).ToList();

            return JsonSerializer.Serialize(new
            {
                success = true,
                count = result.Count,
                products = result
            });
        }

        [KernelFunction("check_product_stock")]
        [Description("Kiem tra tinh trang ton kho cua san pham theo ID. Tra ve so luong con lai va trang thai con hang hay het.")]
        public async Task<string> CheckProductStockAsync(
            [Description("ID cua san pham can kiem tra ton kho")]
            int productId)
        {
            var product = await _productService.GetByIdAsync(productId);

            if (product == null)
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    message = $"Khong tim thay san pham voi ID {productId}"
                });

            return JsonSerializer.Serialize(new
            {
                success = true,
                productId = product.Id,
                productName = product.Name,
                stock = product.Stock,
                inStock = product.Stock > 0,
                message = product.Stock > 0
                    ? $"{product.Name} con {product.Stock} san pham trong kho"
                    : $"{product.Name} hien da het hang"
            });
        }

        [KernelFunction("suggest_project_components")]
        [Description("Goi y cac linh kien can thiet cho mot du an/project dien tu (VD: xe tu hanh, nha thong minh, robot). Dung LLM de xac dinh linh kien can co, sau do tim trong cua hang va de xuat cho khach.")]
        public async Task<string> SuggestProjectComponentsAsync(
            [Description("Mo ta du an cua khach hang (VD: 'xe tu hanh', 'nha thong minh', 'robot do duong')")]
            string projectDescription,

            [Description("Ngan sach toi da (VND), de trong neu khong gioi han")]
            decimal? maxBudget = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(projectDescription))
                {
                    return JsonSerializer.Serialize(new
                    {
                        success = false,
                        message = "Mo ta du an khong duoc de trong."
                    });
                }

                var llmPlan = await AnalyzeProjectWithLlmAsync(projectDescription);

                var queries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var q in llmPlan.RequiredComponents.Select(x => x.Keyword))
                    if (!string.IsNullOrWhiteSpace(q)) queries.Add(q.Trim());

                foreach (var q in llmPlan.Keywords)
                    if (!string.IsNullOrWhiteSpace(q)) queries.Add(q.Trim());

                foreach (var q in projectDescription.Split(' ', ',', '-', '.', '!', '?')
                    .Where(x => x.Length > 2))
                    queries.Add(q);

                queries.Add(projectDescription);

                var allProducts = new List<ProductDto>();
                var score = new Dictionary<int, int>();

                foreach (var query in queries.Take(20))
                {
                    var results = await _productService.SearchProductsAsync(query, maxBudget, 8);
                    foreach (var p in results)
                    {
                        allProducts.Add(p);
                        score[p.Id] = score.TryGetValue(p.Id, out var s) ? s + 1 : 1;
                    }
                }

                var uniqueProducts = allProducts
                    .Where(p => p.Stock > 0)
                    .GroupBy(p => p.Id)
                    .Select(g => g.First())
                    .OrderByDescending(p => score.TryGetValue(p.Id, out var s) ? s : 0)
                    .ThenBy(p => p.Price)
                    .Take(20)
                    .ToList();

                if (!uniqueProducts.Any())
                {
                    return JsonSerializer.Serialize(new
                    {
                        success = false,
                        message = $"Khong tim thay linh kien phu hop cho du an '{projectDescription}' trong cua hang.",
                        llmRequiredComponents = llmPlan.RequiredComponents.Select(x => x.Name).ToList(),
                        suggestion = "Ban co the mo ta chi tiet hon (vi du: can ket noi wifi, dieu khien dong co, cam bien nao)."
                    });
                }

                var totalPrice = uniqueProducts.Sum(p => p.Price);
                var components = uniqueProducts.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    price = p.Price,
                    stock = p.Stock,
                    category = p.CategoryName,
                    brand = p.BrandName,
                    score = score.TryGetValue(p.Id, out var s) ? s : 0
                }).ToList();

                return JsonSerializer.Serialize(new
                {
                    success = true,
                    projectDescription,
                    llmAnalysis = new
                    {
                        projectType = llmPlan.ProjectType,
                        keywords = llmPlan.Keywords,
                        requiredComponents = llmPlan.RequiredComponents
                    },
                    message = $"Tim thay {components.Count} linh kien phu hop trong cua hang cho du an '{projectDescription}'",
                    totalEstimatedPrice = totalPrice,
                    maxBudget,
                    withinBudget = maxBudget == null || totalPrice <= maxBudget,
                    components,
                    note = "Neu muon them vao gio: 'them san pham ID [so] vao gio'."
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    message = $"Loi khi tu van linh kien: {ex.Message}"
                });
            }
        }

        private async Task<ProjectAnalyzeResult> AnalyzeProjectWithLlmAsync(string projectDescription)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return BuildFallback(projectDescription);
            }

            try
            {
                var builder = Kernel.CreateBuilder();
                builder.AddGoogleAIGeminiChatCompletion(modelId: "gemini-2.5-flash", apiKey: apiKey);
                var kernel = builder.Build();

                var prompt = $@"
Ban la ky su dien tu. 
Phan tich yeu cau du an va tra ve JSON THUAN (khong markdown), schema:
{{
  ""projectType"": ""..."",
  ""keywords"": [""..."", ""...""],
  ""requiredComponents"": [
    {{ ""name"": ""..."", ""keyword"": ""..."" }}
  ]
}}
Nguoi dung yeu cau: ""{projectDescription}""
Chi tra ve JSON.";

                var settings = new GeminiPromptExecutionSettings
                {
                    Temperature = 0.2,
                    MaxTokens = 500
                };

                using var cts = new CancellationTokenSource(_timeout);
                var result = await kernel.InvokePromptAsync(prompt, new(settings), cancellationToken: cts.Token);
                var json = CleanJson(result.ToString());

                var parsed = JsonSerializer.Deserialize<ProjectAnalyzeResult>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (parsed == null || !parsed.RequiredComponents.Any())
                    return BuildFallback(projectDescription);

                return parsed;
            }
            catch
            {
                return BuildFallback(projectDescription);
            }
        }

        private static ProjectAnalyzeResult BuildFallback(string projectDescription)
        {
            var keywords = projectDescription
                .ToLower()
                .Split(' ', ',', '-', '.', '!', '?')
                .Where(x => x.Length > 2)
                .Distinct()
                .Take(8)
                .ToList();

            return new ProjectAnalyzeResult
            {
                ProjectType = "general",
                Keywords = keywords,
                RequiredComponents = keywords.Select(x => new RequiredComponent
                {
                    Name = x,
                    Keyword = x
                }).ToList()
            };
        }

        private static string CleanJson(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "{}";

            text = text.Trim();
            if (text.StartsWith("```json")) text = text[7..];
            if (text.StartsWith("```")) text = text[3..];
            if (text.EndsWith("```")) text = text[..^3];
            return text.Trim();
        }

        private sealed class ProjectAnalyzeResult
        {
            public string ProjectType { get; set; } = "general";
            public List<string> Keywords { get; set; } = new();
            public List<RequiredComponent> RequiredComponents { get; set; } = new();
        }

        private sealed class RequiredComponent
        {
            public string Name { get; set; } = "";
            public string Keyword { get; set; } = "";
        }
    }
}
