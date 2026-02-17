using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Application.DTOs;

namespace WebApp.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // Hiển thị giỏ hàng
        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            return View(cart);
        }

        // Thêm sản phẩm vào giỏ
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

        // Cập nhật số lượng
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

        // Xóa sản phẩm khỏi giỏ
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

        // Xóa toàn bộ giỏ hàng
        [HttpPost]
        public IActionResult ClearCart()
        {
            _cartService.ClearCart();
            return RedirectToAction("Index");
        }

        // Lấy số lượng trong giỏ
        [HttpGet]
        public IActionResult GetCartCount()
        {
            return Json(new { count = _cartService.GetCartCount() });
        }
    }
}
