using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Orders;
using Microsoft.EntityFrameworkCore;
using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;

namespace WebApp.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly TechStore.Infrastructure.Data.ApplicationDbContext _context;

        public CartController(ICartService cartService, TechStore.Infrastructure.Data.ApplicationDbContext context)
        {
            _cartService = cartService;
            _context = context;
        }

        // Hi?n th? gi? hàng
        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            return View(cart);
        }

        // Thêm s?n ph?m vào gi?
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var productInfo = await _cartService.GetProductForCartAsync(productId);

            if (productInfo == null)
            {
                return NotFound();
            }

            var cart = _cartService.GetCart();
            var existingItem = cart.FirstOrDefault(x => x.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                productInfo.Quantity = quantity;
                cart.Add(productInfo);
            }

            _cartService.SaveCart(cart);

            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return Json(new { success = true, cartCount = _cartService.GetCartCount() });
            }

            return RedirectToAction("Index");
        }

        // C?p nh?t s? l??ng
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = _cartService.GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                if (quantity <= 0)
                    cart.Remove(item);
                else
                    item.Quantity = quantity;
                
                _cartService.SaveCart(cart);
            }

            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return Json(new { 
                    success = true, 
                    cartCount = _cartService.GetCartCount(),
                    cartTotal = _cartService.GetCartTotal()
                });
            }

            return RedirectToAction("Index");
        }

        // Xóa s?n ph?m kh?i gi?
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = _cartService.GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                cart.Remove(item);
                _cartService.SaveCart(cart);
            }

            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return Json(new { success = true, cartCount = _cartService.GetCartCount() });
            }

            return RedirectToAction("Index");
        }

        // Xóa toàn b? gi? hàng
        [HttpPost]
        public IActionResult ClearCart()
        {
            _cartService.ClearCart();
            return RedirectToAction("Index");
        }

        // L?y s? l??ng trong gi?
        [HttpGet]
        public IActionResult GetCartCount()
        {
            return Json(new { count = _cartService.GetCartCount() });
        }

        // Áp mã giảm giá (lưu vào session)
        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Json(new { success = false, message = "Mã rỗng" });
            }

            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == code && c.IsActive && !c.IsDeleted);
            if (coupon == null)
            {
                return Json(new { success = false, message = "Mã không tồn tại hoặc đã bị tắt" });
            }

            var now = DateTime.UtcNow;
            if (coupon.StartDate.HasValue && coupon.StartDate.Value > now)
                return Json(new { success = false, message = "Mã chưa có hiệu lực" });
            if (coupon.EndDate.HasValue && coupon.EndDate.Value < now)
                return Json(new { success = false, message = "Mã đã hết hạn" });
            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
                return Json(new { success = false, message = "Mã đã đạt giới hạn sử dụng" });

            var cartTotal = _cartService.GetCartTotal();
            decimal discountAmount = coupon.IsPercent ? Math.Round(cartTotal * (coupon.Amount / 100m), 2) : coupon.Amount;
            if (discountAmount > cartTotal) discountAmount = cartTotal;

            // save coupon code in session
            _cartService.SetCouponCode(code);

            return Json(new
            {
                success = true,
                message = coupon.IsPercent ? $"Áp dụng: {coupon.Amount}%" : $"Áp dụng: {coupon.Amount:N0}₫",
                discountAmount,
                discountedTotal = cartTotal - discountAmount
            });
        }

        [HttpPost]
        public IActionResult RemoveCoupon()
        {
            _cartService.SetCouponCode(null);
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetAppliedCoupon()
        {
            var code = _cartService.GetCouponCode();
            if (string.IsNullOrWhiteSpace(code)) return Json(new { applied = false });

            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == code && c.IsActive && !c.IsDeleted);
            if (coupon == null) return Json(new { applied = false });

            var cartTotal = _cartService.GetCartTotal();
            decimal discountAmount = coupon.IsPercent ? Math.Round(cartTotal * (coupon.Amount / 100m), 2) : coupon.Amount;
            if (discountAmount > cartTotal) discountAmount = cartTotal;

            return Json(new { applied = true, code = coupon.Code, discountAmount, discountedTotal = cartTotal - discountAmount });
        }
    }
}
