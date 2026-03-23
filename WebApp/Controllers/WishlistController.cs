using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Catalog;
using System.Threading.Tasks;

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
            var items = await _wishlistService.GetWishlistProductsAsync();
            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId)
        {
            var result = await _wishlistService.AddProductAsync(productId);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = result.Success, message = result.Message, count = result.Count });
            }

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var count = _wishlistService.RemoveProduct(productId);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, count = count });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Clear()
        {
            _wishlistService.Clear();
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, count = 0 });
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GetCount()
        {
            return Json(new { count = _wishlistService.GetCount() });
        }

        [HttpGet]
        public IActionResult GetIds()
        {
            var ids = _wishlistService.GetWishlistProductIds();
            return Json(ids);
        }
    }
}
