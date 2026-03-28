using System.Security.Claims;
using Application.Interfaces.Catalog;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl = Url.Action("Index", "Wishlist") });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var items = await _wishlistService.GetItemsAsync(userId);
            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                var referer = Request.Headers.Referer.ToString();
                if (string.IsNullOrWhiteSpace(referer))
                {
                    referer = Url.Action("Index", "Home") ?? "/";
                }

                var loginUrl = Url.Page("/Account/Login", new { area = "Identity", returnUrl = referer });

                return Json(new
                {
                    success = false,
                    requireLogin = true,
                    message = "Vui lòng đăng nhập để thêm wishlist.",
                    loginUrl
                });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _wishlistService.AddAsync(userId, productId);

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                wishlistCount = result.Count
            });
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Json(new { success = false, message = "Bạn chưa đăng nhập.", wishlistCount = 0 });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _wishlistService.RemoveAsync(userId, productId);

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                wishlistCount = result.Count
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCount()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Json(new { count = 0 });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var count = await _wishlistService.GetCountAsync(userId);
            return Json(new { count });
        }
    }
}