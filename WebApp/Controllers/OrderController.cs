using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;

namespace WebApp.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly IGHNService _ghnService;
        private readonly TechStore.Infrastructure.Data.ApplicationDbContext _context;

        public OrderController(
            IOrderService orderService,
            ICartService cartService,
            IGHNService ghnService,
            TechStore.Infrastructure.Data.ApplicationDbContext context)
        {
            _orderService = orderService;
            _cartService = cartService;
            _ghnService = ghnService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(CancellationToken cancellationToken)
        {
            var cartItems = _cartService.GetCart();
            if (cartItems == null || !cartItems.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.CartItems = cartItems;
            var cartTotal = cartItems.Sum(x => x.Total);
            ViewBag.CartTotal = cartTotal;

            // Check if a coupon code is saved in session and compute discount for display
            var appliedCode = _cartService.GetCouponCode();
            decimal discountAmount = 0m;
            decimal discountedTotal = cartTotal;
            string? couponMessage = null;

            if (!string.IsNullOrWhiteSpace(appliedCode))
            {
                var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == appliedCode && c.IsActive && !c.IsDeleted, cancellationToken);
                if (coupon != null)
                {
                    var now = DateTime.UtcNow;
                    if (!(coupon.StartDate.HasValue && coupon.StartDate.Value > now) && !(coupon.EndDate.HasValue && coupon.EndDate.Value < now) && !(coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value))
                    {
                        discountAmount = coupon.IsPercent ? Math.Round(cartTotal * (coupon.Amount / 100m), 2) : coupon.Amount;
                        if (discountAmount > cartTotal) discountAmount = cartTotal;
                        discountedTotal = cartTotal - discountAmount;
                        couponMessage = coupon.IsPercent ? $"Áp dụng: {coupon.Amount}%" : $"Áp dụng: {coupon.Amount:N0}₫";
                    }
                }
            }

            ViewBag.Provinces = await _ghnService.GetProvincesAsync(cancellationToken);
            ViewBag.DiscountAmount = discountAmount;
            ViewBag.DiscountedTotal = discountedTotal;
            ViewBag.CouponMessage = couponMessage;

            var model = new CheckoutDto();
            model.CouponCode = appliedCode;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutDto model, CancellationToken cancellationToken)
        {
            var cartItems = _cartService.GetCart();

            if (!ModelState.IsValid)
            {
                ViewBag.CartItems = cartItems;
                ViewBag.CartTotal = cartItems.Sum(x => x.Total);
                ViewBag.Provinces = await _ghnService.GetProvincesAsync(cancellationToken);
                return View(model);
            }

            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (model.DistrictId <= 0 || string.IsNullOrWhiteSpace(model.WardCode))
            {
                ModelState.AddModelError("", "Vui lòng chọn Tỉnh/Quận/Phường.");
            }

            var insuranceValue = (int)Math.Round(cartItems.Sum(x => x.Total), 0);
            var fee = await _ghnService.GetShippingFeeAsync(model.DistrictId, model.WardCode, insuranceValue, cancellationToken);

            if (!fee.HasValue)
            {
                ModelState.AddModelError("", "Không tính được phí vận chuyển. Vui lòng thử lại.");
            }
            else
            {
                model.ShippingFee = fee.Value;
            }

            // QUAN TRỌNG: validate lần 2 sau khi gọi GHN
            if (!ModelState.IsValid)
            {
                ViewBag.CartItems = cartItems;
                ViewBag.CartTotal = cartItems.Sum(x => x.Total);
                ViewBag.Provinces = await _ghnService.GetProvincesAsync(cancellationToken);
                return View(model);
            }

            var result = await _orderService.CreateOrderAsync(userId, model, cartItems);

            if (result.Success)
            {
                if (string.Equals(model.PaymentMethod, "MoMo", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("ProcessMoMoPayment", "Payment", new { orderId = result.OrderId });
                }

                _cartService.ClearCart();
                return RedirectToAction("Confirmation", new { id = result.OrderId });
            }

            ModelState.AddModelError("", result.ErrorMessage ?? "Đặt hàng thất bại");
            foreach (var msg in result.OutOfStockProducts)
            {
                ModelState.AddModelError("", msg);
            }

            ViewBag.CartItems = cartItems;
            ViewBag.CartTotal = cartItems.Sum(x => x.Total);
            ViewBag.Provinces = await _ghnService.GetProvincesAsync(cancellationToken);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetDistricts(int provinceId, CancellationToken cancellationToken)
        {
            if (provinceId <= 0)
            {
                return Json(Array.Empty<object>());
            }

            var districts = await _ghnService.GetDistrictsAsync(provinceId, cancellationToken);
            return Json(districts.Select(x => new { id = x.DistrictId, name = x.DistrictName }));
        }

        [HttpGet]
        public async Task<IActionResult> GetWards(int districtId, CancellationToken cancellationToken)
        {
            if (districtId <= 0)
            {
                return Json(Array.Empty<object>());
            }

            var wards = await _ghnService.GetWardsAsync(districtId, cancellationToken);
            return Json(wards.Select(x => new { code = x.WardCode, name = x.WardName }));
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id, User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (order == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _orderService.GetUserOrdersAsync(userId);
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _orderService.GetOrderByIdAsync(id, userId);
            if (order == null)
            {
                return RedirectToAction("MyOrders");
            }
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var success = await _orderService.CancelOrderAsync(id, userId);
            if (!success)
            {
                TempData["Error"] = "Không thể hủy đơn hàng này.";
                return RedirectToAction("MyOrders");
            }

            TempData["Success"] = "Đơn hàng đã được hủy.";
            return RedirectToAction("MyOrders");
        }

        [HttpGet]
        public async Task<IActionResult> GetShippingFee(int districtId, string wardCode, CancellationToken cancellationToken)
        {
            if (districtId <= 0 || string.IsNullOrWhiteSpace(wardCode))
            {
                return Json(new { success = false, message = "Thiếu thông tin địa chỉ." });
            }

            var cartItems = _cartService.GetCart();
            var insuranceValue = (int)Math.Round(cartItems.Sum(x => x.Total), 0);

            var fee = await _ghnService.GetShippingFeeAsync(districtId, wardCode, insuranceValue, cancellationToken);
            if (!fee.HasValue)
            {
                return Json(new { success = false, message = "Không lấy được phí vận chuyển." });
            }

            return Json(new { success = true, fee = fee.Value });
        }
    }
}
