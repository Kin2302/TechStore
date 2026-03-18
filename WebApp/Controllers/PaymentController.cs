using System.Security.Claims;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStore.Domain.Enums;

namespace WebApp.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IMoMoService _moMoService;
        private readonly ICartService _cartService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IOrderService orderService,
            IMoMoService moMoService,
            ICartService cartService,
            ILogger<PaymentController> logger)
        {
            _orderService = orderService;
            _moMoService = moMoService;
            _cartService = cartService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> ProcessMoMoPayment(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _orderService.GetOrderByIdAsync(orderId, userId);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("MyOrders", "Order");
            }

            var returnUrl = Url.Action(nameof(MoMoReturn), "Payment", null, Request.Scheme);
            var ipnUrl = Url.Action(nameof(MoMoIpn), "Payment", null, Request.Scheme);

            // MoMo orderId phải unique mỗi lần tạo payment
            var momoOrderId = $"{order.Id}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

            var payment = await _moMoService.CreatePaymentAsync(
                orderId: momoOrderId,
                amount: Convert.ToInt64(Math.Round(order.TotalAmount, 0)),
                orderInfo: $"Thanh toan don hang #{order.Id}",
                returnUrl: returnUrl,
                notifyUrl: ipnUrl);

            if (!payment.Success || string.IsNullOrWhiteSpace(payment.PayUrl))
            {
                TempData["Error"] = $"Không tạo được link MoMo: {payment.Message}";
                return RedirectToAction("Details", "Order", new { id = orderId });
            }

            return Redirect(payment.PayUrl);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> MoMoReturn()
        {
            if (!_moMoService.ValidateReturnSignature(Request.Query))
            {
                return BadRequest("Invalid MoMo signature.");
            }

            var resultCode = Request.Query["resultCode"].ToString();
            var message = Request.Query["message"].ToString();

            // orderId từ MoMo có dạng: {internalOrderId}_{timestamp}
            var rawOrderId = Request.Query["orderId"].ToString();
            var internalOrderIdText = rawOrderId.Split('_')[0];

            if (!int.TryParse(internalOrderIdText, out var orderId))
            {
                return BadRequest("Invalid order id.");
            }

            if (resultCode == "0")
            {
                await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Confirmed);
                _cartService.ClearCart();
                TempData["Success"] = "Thanh toán MoMo thành công.";
                return RedirectToAction("Confirmation", "Order", new { id = orderId });
            }

            TempData["Error"] = $"Thanh toán thất bại: {message}";
            return RedirectToAction("Details", "Order", new { id = orderId });
        }

        [AllowAnonymous]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult MoMoIpn()
        {
            _logger.LogInformation("MoMo IPN received.");
            return Ok(new { message = "Received" });
        }
    }
}