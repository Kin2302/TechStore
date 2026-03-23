using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.Interfaces.Integration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using System.Text.Json;

namespace TechStore.Infrastructure.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly IKernelFactory _kernelFactory;
        private readonly ILogger<GeminiService>? _logger;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(60);

        public GeminiService(IKernelFactory kernelFactory, ILogger<GeminiService>? logger = null)
        {
            _kernelFactory = kernelFactory;
            _logger = logger;
            _logger?.LogInformation("GeminiService initialized with KernelFactory");
        }

        public async Task<string> ChatAsync(string userMessage, bool isAdmin = false, string? userId = null)
        {
            var kernel = isAdmin
                ? _kernelFactory.CreateAdminKernel()
                : _kernelFactory.CreateCustomerKernel();

            var systemPrompt = isAdmin
                ? GetAdminSystemPrompt()
                : GetCustomerSystemPrompt();

            // Inject userId info if available
            var userContext = !string.IsNullOrEmpty(userId)
                ? $"\n\nThong tin user hien tai: userId='{userId}'. Su dung userId nay khi goi cac ham lien quan den don hang."
                : "";

            var fullPrompt = $"{systemPrompt}{userContext}\n\nNguoi dung: {userMessage}";

            try
            {
                _logger?.LogInformation("=== ChatAsync Start (isAdmin={IsAdmin}) ===", isAdmin);
                _logger?.LogInformation("User Message: {Message}", userMessage);

                var settings = new GeminiPromptExecutionSettings
                {
                    Temperature = 0.7,
                    MaxTokens = 1500,
                    ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions
                };

                using var cts = new CancellationTokenSource(_timeout);
                var result = await kernel.InvokePromptAsync(fullPrompt, new(settings), cancellationToken: cts.Token);

                var response = result.ToString().Trim();
                _logger?.LogInformation("AI Response: {Length} chars", response.Length);
                return response;
            }
            catch (OperationCanceledException ex)
            {
                _logger?.LogError(ex, "ChatAsync timeout after {Seconds}s", _timeout.TotalSeconds);
                return "He thong AI phan hoi cham, vui long thu lai.";
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogError(ex, "Loi ket noi Gemini API: {Message}", ex.Message);
                return "Khong the ket noi voi AI. Vui long kiem tra ket noi mang.";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Loi ChatAsync: {Message}", ex.Message);
                return $"Xin loi, da xay ra loi: {ex.Message}";
            }
        }

        private string GetCustomerSystemPrompt()
        {
            return @"
Ban la tro ly AI thong minh cua TechStore - cua hang linh kien dien tu.

NHIEM VU:
- Ho tro khach hang tim san pham theo yeu cau
- Tu van linh kien phu hop voi du an/nhu cau
- Goi y linh kien can thiet cho cac project (xe tu hanh, nha thong minh, robot...)
- Them san pham vao gio hang khi khach yeu cau
- Quan ly gio hang (xem, xoa san pham)
- Kiem tra ton kho san pham
- Xem lich su don hang va trang thai don hang
- Cung cap thong tin cua hang va chinh sach bao hanh

QUY TAC:
1. CHI goi y san pham co trong cua hang (su dung search_products hoac suggest_project_components)
2. Gia PHAI chinh xac tu du lieu
3. Khi khach noi 'them vao gio' hoac 'mua' -> GOI add_to_cart
4. Khi khach hoi ve du an/project -> GOI suggest_project_components de tim linh kien phu hop
5. Tra loi tieng Viet tu nhien, than thien
6. Neu khong chac, HOI THEM thong tin
7. Khi khach muon xem don hang -> GOI get_order_history hoac get_order_status (can userId)

TOOLS CO SAN:
- search_products: Tim san pham theo tu khoa
- get_product_details: Xem chi tiet san pham
- get_popular_products: Lay san pham ban chay
- check_product_stock: Kiem tra ton kho
- suggest_project_components: Goi y linh kien cho project/du an
- add_to_cart: Them vao gio hang
- remove_from_cart: Xoa khoi gio
- view_cart: Xem gio hang
- clear_cart: Xoa toan bo gio
- update_cart_quantity: Cap nhat so luong
- get_order_history: Xem lich su don hang
- get_order_status: Kiem tra trang thai don hang
- get_store_info: Thong tin cua hang
- get_warranty_policy: Chinh sach bao hanh

CACH XU LY:
- 'Tim Arduino' -> search_products(keyword='Arduino')
- 'Toi muon lam xe tu hanh' -> suggest_project_components(projectDescription='xe tu hanh')
- 'Them Arduino Uno vao gio' -> search_products -> add_to_cart
- 'Gio hang cua toi' -> view_cart
- 'Don hang cua toi' -> get_order_history(userId)
- 'Trang thai don hang #5' -> get_order_status(orderId=5, userId)
- 'Dia chi cua hang' -> get_store_info
- 'Chinh sach bao hanh' -> get_warranty_policy";
        }

        private string GetAdminSystemPrompt()
        {
            return @"
Ban la tro ly AI quan tri (Admin Assistant) cua TechStore - cua hang linh kien dien tu.

NHIEM VU:
- Bao cao doanh thu, thong ke don hang
- Quan ly san pham va kho hang
- Tim kiem thong tin khach hang
- Cap nhat trang thai don hang
- Ho tro tat ca chuc nang cua khach hang (tim SP, gio hang...)

QUY TAC:
1. Tra loi chinh xac, chuyen nghiep
2. San pham tao qua chat se mac dinh CHUA HIEN THI (IsActive=false)
3. Thong ke phai chinh xac tu du lieu
4. Tra loi tieng Viet

TOOLS CO SAN (ADMIN):
- get_revenue_report: Bao cao doanh thu theo khoang thoi gian
- get_best_selling_products: Top san pham ban chay
- get_order_statistics: Thong ke don hang theo trang thai
- create_product: Tao san pham moi
- create_multiple_products: Tao nhieu san pham
- get_low_stock_products: San pham sap het hang
- update_product_stock: Cap nhat ton kho
- update_order_status: Cap nhat trang thai don hang
- search_users: Tim kiem khach hang

TOOLS CUSTOMER (cung co san):
- search_products, get_product_details, get_popular_products
- check_product_stock, suggest_project_components
- add_to_cart, remove_from_cart, view_cart, clear_cart, update_cart_quantity
- get_order_history, get_order_status
- get_store_info, get_warranty_policy

CACH XU LY:
- 'Doanh thu thang nay' -> get_revenue_report(startDate, endDate)
- 'Top 5 ban chay' -> get_best_selling_products(limit=5)
- 'San pham sap het hang' -> get_low_stock_products(threshold=10)
- 'Cap nhat ton kho SP #1 thanh 50' -> update_product_stock(1, 50)
- 'Xac nhan don #1204' -> update_order_status(1204, 'Confirmed')
- 'Tim khach abc@gmail.com' -> search_users('abc@gmail.com')";
        }

        public async Task<AnalysisResultDto> AnalyzeQueryAsync(string userMessage)
        {
            var kernel = _kernelFactory.CreateCustomerKernel();

            var prompt = $@"
Phan tich cau hoi cua khach hang ve linh kien dien tu.
Cau hoi: ""{userMessage}""

Yeu cau tra ve JSON mau:
{{
  ""keywords"": [""arduino"", ""uno""],
  ""categories"": [""Board Mach""],
  ""intent"": ""project""
}}

Danh sach Intent: project (tu van du an), question (hoi dap thong thuong), price (hoi gia).
Chi tra ve JSON thuan, khong kem Markdown.";

            try
            {
                var settings = new GeminiPromptExecutionSettings
                {
                    Temperature = 0.2,
                    MaxTokens = 150
                };

                using var cts = new CancellationTokenSource(_timeout);
                var result = await kernel.InvokePromptAsync(prompt, new(settings), cancellationToken: cts.Token);
                var jsonText = CleanJsonString(result.ToString());

                if (string.IsNullOrEmpty(jsonText))
                    return new AnalysisResultDto();

                return JsonSerializer.Deserialize<AnalysisResultDto>(jsonText,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new AnalysisResultDto();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Loi AnalyzeQueryAsync");
                return new AnalysisResultDto { Intent = "question" };
            }
        }

        public async Task<ProjectSuggestionDto?> SuggestProjectAsync(string userRequest, List<ProductInfoDto> products)
        {
            if (!products.Any()) return null;

            var kernel = _kernelFactory.CreateCustomerKernel();

            var productListText = string.Join("\n", products.Take(30).Select(p =>
                $"- ID:{p.Id} | {p.Name} | {p.Price:N0}d"));

            var prompt = $@"
Danh sach linh kien cua hang co:
{productListText}

Khach hang muon: ""{userRequest}""

Hay dong vai chuyen gia ky thuat, de xuat mot du an cu the.
Tra ve JSON mau:
{{
  ""projectName"": ""Ten du an"",
  ""description"": ""Mo ta cong nang ngan gon"",
  ""totalBudget"": 500000,
  ""difficulty"": ""Trung binh"",
  ""tips"": ""Luu y khi lam"",
  ""missingItems"": [""Ten cac mon can mua them o ngoai neu cua hang khong co""],
  ""components"": [
    {{ ""productId"": 1, ""name"": ""Ten linh kien lay tu danh sach"", ""quantity"": 1, ""price"": 100000, ""reason"": ""Dung de lam gi"" }}
  ]
}}
Chi chon linh kien co trong danh sach cung cap.";

            try
            {
                var settings = new GeminiPromptExecutionSettings
                {
                    Temperature = 0.5,
                    MaxTokens = 1000
                };

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                var result = await kernel.InvokePromptAsync(prompt, new(settings), cancellationToken: cts.Token);
                var jsonText = CleanJsonString(result.ToString());

                if (string.IsNullOrEmpty(jsonText)) return null;

                return JsonSerializer.Deserialize<ProjectSuggestionDto>(jsonText,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Loi SuggestProjectAsync");
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
