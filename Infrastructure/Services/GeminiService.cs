using Application.DTOs;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;
using System.Text.Json;

namespace TechStore.Infrastructure.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly GenerativeModel? _model;
        private readonly ILogger<GeminiService>? _logger;
        private readonly string _modelName = "gemini-2.5-flash"; // Đổi sang model ổn định hơn
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);

        public GeminiService(IConfiguration configuration, ILogger<GeminiService>? logger = null)
        {
            _logger = logger;
            var apiKey = configuration["Gemini:ApiKey"];
            
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger?.LogWarning("Gemini API Key chưa được cấu hình!");
                return;
            }

            try
            {
                var googleAI = new GoogleAI(apiKey);
                _model = googleAI.GenerativeModel(model: _modelName);
                _logger?.LogInformation("Khởi tạo Gemini model '{Model}' thành công", _modelName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Lỗi khởi tạo Gemini model");
            }
        }

        public async Task<string> ChatAsync(string userMessage, string productContext)
        {
            if (_model == null) return "⚠️ Hệ thống AI chưa cấu hình.";

            var prompt = $@"
Bạn là trợ lý AI của TechStore - cửa hàng linh kiện điện tử.

📦 SẢN PHẨM HIỆN CÓ:
{productContext}

QUY TẮC: 
1. CHỈ gợi ý sản phẩm có trong danh sách trên.
2. Giá phải lấy CHÍNH XÁC từ dữ liệu.
3. Trả lời tiếng Việt tự nhiên, ngắn gọn (2-4 câu).

Khách hàng: {userMessage}";

            try
            {
                _logger?.LogInformation("Gửi ChatAsync request...");
                
                using var cts = new CancellationTokenSource(_timeout);
                var response = await _model.GenerateContent(prompt).WaitAsync(cts.Token);
                
                var result = response.Text?.Trim();
                _logger?.LogInformation("Nhận response: {Length} ký tự", result?.Length ?? 0);
                
                return result ?? "Xin lỗi, tôi không hiểu câu hỏi.";
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("ChatAsync timeout sau {Seconds}s", _timeout.TotalSeconds);
                return "⏱️ Hệ thống AI phản hồi chậm, vui lòng thử lại.";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Lỗi ChatAsync: {Message}", ex.Message);
                return $"Lỗi kết nối AI: {ex.Message}";
            }
        }

        public async Task<AnalysisResultDto> AnalyzeQueryAsync(string userMessage)
        {
            if (_model == null) return new AnalysisResultDto();

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

                var config = new GenerationConfig
                {
                    ResponseMimeType = "application/json",
                    MaxOutputTokens = 150,
                    Temperature = 0.2f
                };

                using var cts = new CancellationTokenSource(_timeout);
                var response = await _model.GenerateContent(prompt, config).WaitAsync(cts.Token);
                var jsonText = CleanJsonString(response.Text);

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
            if (_model == null || !products.Any()) return null;

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

                var config = new GenerationConfig
                {
                    ResponseMimeType = "application/json",
                    MaxOutputTokens = 1000,
                    Temperature = 0.5f
                };

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60)); // Timeout dài hơn
                var response = await _model.GenerateContent(prompt, config).WaitAsync(cts.Token);
                var jsonText = CleanJsonString(response.Text);

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