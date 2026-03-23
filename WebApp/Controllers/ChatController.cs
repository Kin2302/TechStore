using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Integration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IGeminiService _geminiService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IGeminiService geminiService, ILogger<ChatController> logger)
        {
            _geminiService = geminiService;
            _logger = logger;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequestModel request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new { success = false, message = "Message is required" });
                }

                _logger.LogInformation("=== Chat Request Start ===");
                _logger.LogInformation("User message: {Message}", request.Message);

                // Detect role and userId
                var isAdmin = User.IsInRole("Admin");
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                _logger.LogInformation("IsAdmin: {IsAdmin}, UserId: {UserId}", isAdmin, userId);

                var result = await _geminiService.ChatAsync(request.Message, isAdmin, userId);

                return Ok(new ChatResponseModel
                {
                    Reply = result,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Chat: {Message}", ex.Message);
                return Ok(new ChatResponseModel
                {
                    Reply = $"Xin loi, da xay ra loi: {ex.Message}",
                    Success = false
                });
            }
        }
    }

    public class ChatRequestModel
    {
        public string Message { get; set; } = string.Empty;
    }

    public class ChatResponseModel
    {
        public string Reply { get; set; } = string.Empty;
        public bool Success { get; set; }
    }
}
