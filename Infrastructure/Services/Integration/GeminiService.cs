using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using System.Text.Json;

namespace TechStore.Infrastructure.Services
{
    public class GeminiService : IGeminiService 
    {       
        private readonly Kernel _kernel;
        private readonly ILogger<GeminiService>? _logger;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(60);

        public GeminiService(Kernel kernel, ILogger<GeminiService>? logger = null)
        {
            _kernel = kernel;
            _logger = logger;
            _logger?.LogInformation("? Kh?i t?o GeminiService v?i Semantic Kernel");
        }

        public async Task<string> ChatAsync(string userMessage)
        {
            var systemPrompt = @"
B?n là tr? lý AI thông minh c?a TechStore - c?a hàng linh ki?n di?n t?.

NHI?M V?:
? H? tr? khách hàng tìm s?n ph?m theo yêu c?u
? Tu v?n linh ki?n phù h?p v?i d? án/nhu c?u
? Thêm s?n ph?m vào gi? hàng khi khách yêu c?u
? Qu?n lý gi? hàng (xem, xóa s?n ph?m)
? Tr? l?i các câu h?i k? thu?t v? linh ki?n di?n t?

QUY T?C:
1. CH? g?i ý s?n ph?m có trong c?a hàng (s? d?ng search_products)
2. Giá PH?I chính xác t? d? li?u
3. Khi khách nói 'thêm vào gi?' ho?c 'mua' ? G?I add_to_cart
4. Tr? l?i ti?ng Vi?t t? nhiên, thân thi?n
5. N?u không ch?c, H?I THÊM thông tin

TOOLS:
- search_products: Tìm s?n ph?m theo t? khóa
- get_product_details: Xem chi ti?t s?n ph?m
- get_popular_products: L?y s?n ph?m bán ch?y
- add_to_cart: Thêm vào gi? hàng
- remove_from_cart: Xóa kh?i gi?
- view_cart: Xem gi? hàng
- clear_cart: Xóa toàn b? gi?
- update_cart_quantity: C?p nh?t s? lu?ng

CÁCH X? LÝ:
- 'Tìm Arduino' ? search_products(keyword='Arduino')
- 'Thêm Arduino Uno vào gi?' ? search_products ? add_to_cart
- 'Gi? hàng c?a tôi' ? view_cart
- 'Xóa s?n ph?m ID 5' ? remove_from_cart(productId=5)";

            var fullPrompt = $"{systemPrompt}\n\nKhách hàng: {userMessage}";

            try
            {
                _logger?.LogInformation("=== ChatAsync Start ===");
                _logger?.LogInformation("User Message: {Message}", userMessage);
                
                var settings = new GeminiPromptExecutionSettings
                {
                    Temperature = 0.7,
                    MaxTokens = 1000,
                    ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions
                };

                _logger?.LogInformation("Sending request to Gemini API...");
                using var cts = new CancellationTokenSource(_timeout);
                
                var result = await _kernel.InvokePromptAsync(fullPrompt, new(settings), cancellationToken: cts.Token);
                
                var response = result.ToString().Trim();
                _logger?.LogInformation("AI Response received: {Length} ký t?", response.Length);
                _logger?.LogInformation("=== ChatAsync Success ===");
                
                return response;
            }
            catch (OperationCanceledException ex)
            {
                _logger?.LogError(ex, "ChatAsync timeout sau {Seconds}s", _timeout.TotalSeconds);
                return "?? H? th?ng AI ph?n h?i ch?m, vui lòng th? l?i.";
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogError(ex, "L?i k?t n?i Gemini API: {Message}", ex.Message);
                return "?? Không th? k?t n?i v?i AI. Vui lòng ki?m tra k?t n?i m?ng.";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "L?i ChatAsync: {Message} | StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                return $"?? Xin l?i, dã x?y ra l?i: {ex.Message}";
            }
        }

        public async Task<AnalysisResultDto> AnalyzeQueryAsync(string userMessage)
        {
            var prompt = $@"
Phân tích câu h?i c?a khách hàng v? linh ki?n di?n t?.
Câu h?i: ""{userMessage}""

Yêu c?u tr? v? JSON m?u:
{{
  ""keywords"": [""arduino"", ""uno""],
  ""categories"": [""Board M?ch""],
  ""intent"": ""project""
}}

Danh sách Intent: project (tu v?n d? án), question (h?i dáp thông thu?ng), price (h?i giá).
Ch? tr? v? JSON thu?n, không kèm Markdown.";

            try
            {
                _logger?.LogInformation("G?i AnalyzeQueryAsync request...");

                var settings = new GeminiPromptExecutionSettings
                {
                    Temperature = 0.2,
                    MaxTokens = 150
                };

                using var cts = new CancellationTokenSource(_timeout);
                var result = await _kernel.InvokePromptAsync(prompt, new(settings), cancellationToken: cts.Token);
                var jsonText = CleanJsonString(result.ToString());

                _logger?.LogInformation("Analyze response: {Json}", jsonText);

                if (string.IsNullOrEmpty(jsonText))
                    return new AnalysisResultDto();

                return JsonSerializer.Deserialize<AnalysisResultDto>(jsonText,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new AnalysisResultDto();
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("AnalyzeQueryAsync timeout");
                return new AnalysisResultDto { Intent = "question" };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "L?i AnalyzeQueryAsync");
                return new AnalysisResultDto { Intent = "question" };
            }
        }

        public async Task<ProjectSuggestionDto?> SuggestProjectAsync(string userRequest, List<ProductInfoDto> products)
        {
            if (!products.Any()) return null;

            var productListText = string.Join("\n", products.Take(30).Select(p =>
                $"- ID:{p.Id} | {p.Name} | {p.Price:N0}d"));

            var prompt = $@"
Danh sách linh ki?n c?a hàng có:
{productListText}

Khách hàng mu?n: ""{userRequest}""

Hãy dóng vai chuyên gia k? thu?t, d? xu?t m?t d? án c? th?.
Tr? v? JSON m?u:
{{
  ""projectName"": ""Tên d? án"",
  ""description"": ""Mô t? công nang ng?n g?n"",
  ""totalBudget"": 500000,
  ""difficulty"": ""Trung bình"",
  ""tips"": ""Luu ý khi làm"",
  ""missingItems"": [""Tên các món c?n mua thêm ? ngoài n?u c?a hàng không có""],
  ""components"": [
    {{ ""productId"": 1, ""name"": ""Tên linh ki?n l?y t? danh sách"", ""quantity"": 1, ""price"": 100000, ""reason"": ""Dùng d? làm gì"" }}
  ]
}}
Ch? ch?n linh ki?n có trong danh sách cung c?p.";

            try
            {
                _logger?.LogInformation("G?i SuggestProjectAsync request...");

                var settings = new GeminiPromptExecutionSettings
                {
                    Temperature = 0.5,
                    MaxTokens = 1000
                };

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                var result = await _kernel.InvokePromptAsync(prompt, new(settings), cancellationToken: cts.Token);
                var jsonText = CleanJsonString(result.ToString());

                _logger?.LogInformation("Project response: {Length} ký t?", jsonText?.Length ?? 0);

                if (string.IsNullOrEmpty(jsonText)) return null;

                return JsonSerializer.Deserialize<ProjectSuggestionDto>(jsonText,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "L?i SuggestProjectAsync");
                return null;
            }
        }

        private string? CleanJsonString(string? text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            text = text.Trim();
            if (text.StartsWith("```json"))
                text = text[7..];
            if (text.StartsWith("```"))
                text = text[3..];
            if (text.EndsWith("```"))
                text = text[..^3];

            return text.Trim();
        }
    }
}       