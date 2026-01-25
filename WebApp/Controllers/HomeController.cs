using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApp.Models;
using Microsoft.EntityFrameworkCore;
using TechStore.Infrastructure.Data;
namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; 
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy danh sách sản phẩm từ SQL
            // Include(p => p.Images): Lấy kèm luôn bảng Ảnh để hiển thị
            // Include(p => p.Category): Lấy kèm tên Danh mục
            var products = await _context.Products
                                         .Include(p => p.Images)
                                         .Include(p => p.Category)
                                         .Take(12) // Lấy tạm 12 cái mới nhất demo
                                         .ToListAsync();

            // Truyền dữ liệu sang View
            return View(products);
        }


        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)              // Kèm thông tin hãng
                .Include(p => p.Images)             // Kèm danh sách ảnh
                .Include(p => p.Specifications)     // Kèm bảng thông số kỹ thuật (Quan trọng!)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(); // Trả về lỗi 404 nếu ko tìm thấy
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
