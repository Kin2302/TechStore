using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;

namespace WebApp.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;

        public OrderController(IOrderService orderService, ICartService cartService)
        {
            _orderService = orderService;
            _cartService = cartService;
        }

        [HttpGet]
        public IActionResult Checkout()
        {
            var cartItems = _cartService.GetCart();
            if (cartItems == null || !cartItems.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.CartItems = cartItems;
            ViewBag.CartTotal = cartItems.Sum(x => x.Total);
            return View(new CheckoutDto());
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutDto model)
        {
            var cartItems = _cartService.GetCart();

            if (!ModelState.IsValid)
            {
                ViewBag.CartItems = cartItems;
                ViewBag.CartTotal = cartItems.Sum(x => x.Total);
                return View(model);
            }

            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

            ModelState.AddModelError("", result.ErrorMessage ?? "Ð?t hàng th?t b?i");
            foreach (var msg in result.OutOfStockProducts)
            {
                ModelState.AddModelError("", msg);
            }

            ViewBag.CartItems = cartItems;
            ViewBag.CartTotal = cartItems.Sum(x => x.Total);
            return View(model);
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
                TempData["Error"] = "Không th? h?y don hàng này.";
                return RedirectToAction("MyOrders");
            }

            TempData["Success"] = "Ðon hàng dã du?c h?y.";
            return RedirectToAction("MyOrders");
        }
    }
}
