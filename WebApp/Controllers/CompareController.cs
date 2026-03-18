using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class CompareController : Controller
    {
        private readonly ICompareService _compareService;

        public CompareController(ICompareService compareService)
        {
            _compareService = compareService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _compareService.GetCompareProductsAsync();
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId)
        {
            var result = await _compareService.AddProductAsync(productId);

            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    compareCount = result.Count
                });
            }

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var count = _compareService.RemoveProduct(productId);

            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return Json(new { success = true, compareCount = count });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Clear()
        {
            _compareService.Clear();

            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return Json(new { success = true, compareCount = 0 });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult GetCount()
        {
            return Json(new { count = _compareService.GetCount() });
        }
    }
}