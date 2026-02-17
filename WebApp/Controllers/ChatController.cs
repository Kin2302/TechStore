using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;

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

        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "healthy", service = "ChatController", timestamp = DateTime.UtcNow });
        }

        [HttpPost("test")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                _logger.LogInformation("=== Test Connection Start ===");
                var result = await _geminiService.ChatAsync("Hello, are you working?");
                return Ok(new { success = true, message = "AI connection OK", response = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test connection failed");
                return Ok(new { 
                    success = false, 
                    error = ex.Message,
                    type = ex.GetType().Name,
                    stackTrace = ex.StackTrace 
                });
            }
        }                   

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequestModel request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { error = "Tin nhắn không được để trống" });

            try
            {
                _logger.LogInformation("User message: {Message}", request.Message);
                
                var reply = await _geminiService.ChatAsync(request.Message);

                return Ok(new ChatResponseModel
                {
                    Reply = reply,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Chat: {Message}", ex.Message);
                return Ok(new ChatResponseModel
                {
                    Reply = $"⚠️ Xin lỗi, đã xảy ra lỗi: {ex.Message}",
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
