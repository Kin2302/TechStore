using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

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

            // Guard 1: Validation fail → return sớm
            if (!ModelState.IsValid)
            {
                ViewBag.CartItems = cartItems;
                ViewBag.CartTotal = cartItems.Sum(x => x.Total);
                return View(model);
            }

            // Guard 2: Cart rỗng → return sớm
            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // Happy path: Tạo order
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _orderService.CreateOrderAsync(userId, model, cartItems);

            if (result.Success)
            {
                _cartService.ClearCart();
                return RedirectToAction("Confirmation", new { id = result.OrderId });
            }

            // Error path: Hiển thị lỗi
            ModelState.AddModelError("", result.ErrorMessage ?? "Đặt hàng thất bại");
            foreach (var msg in result.OutOfStockProducts)
            {
                ModelState.AddModelError("", msg);
            }

            ViewBag.CartItems = cartItems;
            ViewBag.CartTotal = cartItems.Sum(x => x.Total);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(int id )
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


    }
}
