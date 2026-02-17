using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApp.Models;
using Application.Interfaces;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;  // ✅ Dùng Interface

        public HomeController(ILogger<HomeController> logger, IProductService productService)
        {
            _logger = logger;
            _productService = productService;  // ✅ Inject Service
        }

        public async Task<IActionResult> Index()
        {
            // ✅ Gọi qua Service thay vì DbContext trực tiếp
            var products = await _productService.GetFeaturedAsync(12);
            return View(products);
        }

        public async Task<IActionResult> Detail(int id)
        {
            // ✅ Gọi qua Service
            var product = await _productService.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
