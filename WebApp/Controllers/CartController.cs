using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Orders;
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

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
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
    }
}
