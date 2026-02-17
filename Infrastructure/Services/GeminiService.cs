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
            _logger?.LogInformation("✅ Khởi tạo GeminiService với Semantic Kernel");
        }

        public async Task<string> ChatAsync(string userMessage)
        {
            var systemPrompt = @"
Bạn là trợ lý AI thông minh của TechStore - cửa hàng linh kiện điện tử.

NHIỆM VỤ:
✅ Hỗ trợ khách hàng tìm sản phẩm theo yêu cầu
✅ Tư vấn linh kiện phù hợp với dự án/nhu cầu
✅ Thêm sản phẩm vào giỏ hàng khi khách yêu cầu
✅ Quản lý giỏ hàng (xem, xóa sản phẩm)
✅ Trả lời các câu hỏi kỹ thuật về linh kiện điện tử

QUY TẮC:
1. CHỈ gợi ý sản phẩm có trong cửa hàng (sử dụng search_products)
2. Giá PHẢI chính xác từ dữ liệu
3. Khi khách nói 'thêm vào giỏ' hoặc 'mua' → GỌI add_to_cart
4. Trả lời tiếng Việt tự nhiên, thân thiện
5. Nếu không chắc, HỎI THÊM thông tin

TOOLS:
- search_products: Tìm sản phẩm theo từ khóa
- get_product_details: Xem chi tiết sản phẩm
- get_popular_products: Lấy sản phẩm bán chạy
- add_to_cart: Thêm vào giỏ hàng
- remove_from_cart: Xóa khỏi giỏ
- view_cart: Xem giỏ hàng
- clear_cart: Xóa toàn bộ giỏ
- update_cart_quantity: Cập nhật số lượng

CÁCH XỬ LÝ:
- 'Tìm Arduino' → search_products(keyword='Arduino')
- 'Thêm Arduino Uno vào giỏ' → search_products → add_to_cart
- 'Giỏ hàng của tôi' → view_cart
- 'Xóa sản phẩm ID 5' → remove_from_cart(productId=5)";

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
                _logger?.LogInformation("AI Response received: {Length} ký tự", response.Length);
                _logger?.LogInformation("=== ChatAsync Success ===");
                
                return response;
            }
            catch (OperationCanceledException ex)
            {
                _logger?.LogError(ex, "ChatAsync timeout sau {Seconds}s", _timeout.TotalSeconds);
                return "⏱️ Hệ thống AI phản hồi chậm, vui lòng thử lại.";
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogError(ex, "Lỗi kết nối Gemini API: {Message}", ex.Message);
                return "⚠️ Không thể kết nối với AI. Vui lòng kiểm tra kết nối mạng.";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Lỗi ChatAsync: {Message} | StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                return $"⚠️ Xin lỗi, đã xảy ra lỗi: {ex.Message}";
            }
        }

        public async Task<AnalysisResultDto> AnalyzeQueryAsync(string userMessage)
        {
            var prompt = $@"
Phân tích câu hỏi của khách hàng về linh kiện điện tử.
Câu hỏi: ""{userMessage}""

Yêu cầu trả về JSON mẫu:
{{
  ""keywords"": [""arduino"", ""uno""],
  ""categories"": [""Board Mạch""],
  ""intent"": ""project""
}}

Danh sách Intent: project (tư vấn đồ án), question (hỏi đáp thông thường), price (hỏi giá).
Chỉ trả về JSON thuần, không kèm Markdown.";

            try
            {
                _logger?.LogInformation("Gửi AnalyzeQueryAsync request...");

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
                _logger?.LogError(ex, "Lỗi AnalyzeQueryAsync");
                return new AnalysisResultDto { Intent = "question" };
            }
        }

        public async Task<ProjectSuggestionDto?> SuggestProjectAsync(string userRequest, List<ProductInfoDto> products)
        {
            if (!products.Any()) return null;

            var productListText = string.Join("\n", products.Take(30).Select(p =>
                $"- ID:{p.Id} | {p.Name} | {p.Price:N0}đ"));

            var prompt = $@"
Danh sách linh kiện cửa hàng có:
{productListText}

Khách hàng muốn: ""{userRequest}""

Hãy đóng vai chuyên gia kỹ thuật, đề xuất một dự án cụ thể.
Trả về JSON mẫu:
{{
  ""projectName"": ""Tên dự án"",
  ""description"": ""Mô tả công năng ngắn gọn"",
  ""totalBudget"": 500000,
  ""difficulty"": ""Trung bình"",
  ""tips"": ""Lưu ý khi làm"",
  ""missingItems"": [""Tên các món cần mua thêm ở ngoài nếu cửa hàng không có""],
  ""components"": [
    {{ ""productId"": 1, ""name"": ""Tên linh kiện lấy từ danh sách"", ""quantity"": 1, ""price"": 100000, ""reason"": ""Dùng để làm gì"" }}
  ]
}}
Chỉ chọn linh kiện có trong danh sách cung cấp.";

            try
            {
                _logger?.LogInformation("Gửi SuggestProjectAsync request...");

                var settings = new GeminiPromptExecutionSettings
                {
                    Temperature = 0.5,
                    MaxTokens = 1000
                };

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                var result = await _kernel.InvokePromptAsync(prompt, new(settings), cancellationToken: cts.Token);
                var jsonText = CleanJsonString(result.ToString());

                _logger?.LogInformation("Project response: {Length} ký tự", jsonText?.Length ?? 0);

                if (string.IsNullOrEmpty(jsonText)) return null;

                return JsonSerializer.Deserialize<ProjectSuggestionDto>(jsonText,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Lỗi SuggestProjectAsync");
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