using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Application.DTOs;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IGeminiService _geminiService;
        private readonly IProductService _productService;

        public ChatController(IGeminiService geminiService, IProductService productService)
        {
            _geminiService = geminiService;
            _productService = productService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { error = "Tin nhắn không được để trống" });

            // STEP 1: AI phân tích
            var analysis = await _geminiService.AnalyzeQueryAsync(request.Message);

            // STEP 2: Lọc sản phẩm
            var products = await _productService.FilterByAnalysisAsync(analysis);

            if (products.Count < 10)
            {
                var more = await _productService.GetPopularAsync(30 - products.Count);
                products.AddRange(more);
                products = products.DistinctBy(p => p.Id).ToList();
            }

            // STEP 3: Tạo context
            var context = products.Any()
                ? string.Join("\n", products.Select(p => $"- {p.Name}: {p.Price:N0}đ"))
                : "Không có sản phẩm.";

            // STEP 4: AI trả lời
            var reply = await _geminiService.ChatAsync(request.Message, context);

            return Ok(new ChatResponseDto
            {
                Reply = reply,
                Analysis = analysis,
                ProductsFound = products.Count
            });
        }
    }
}