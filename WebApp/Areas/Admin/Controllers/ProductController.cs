using Application.DTOs.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Application.DTOs.Admin;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IAdminProductService _adminProductService;

        public ProductController(IAdminProductService adminProductService)
        {
            _adminProductService = adminProductService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, int? categoryId)
        {
            var products = await _adminProductService.GetAllProductsAsync(search, categoryId);
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            await LoadDropdowns();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View( new ProductCreateDto());
        }
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateDto model , IFormFile? mainImage)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(model);
            }

            var imageUrl = await UploadImageAsync(mainImage);
            await _adminProductService.CreateAsync(model, imageUrl);
            TempData["SuccessMessage"] = "Thêm s?n ph?m thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _adminProductService.GetByIdForEditAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            await LoadDropdowns();
            return View(product);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(ProductEditDto model , IFormFile? mainImage)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(model);
            }
            var imageUrl = await UploadImageAsync(mainImage);
            await _adminProductService.UpdateAsync(model, imageUrl);
            TempData["SuccessMessage"] = "C?p nh?t s?n ph?m thành công!";
            return RedirectToAction(nameof(Index));

        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _adminProductService.DeleteAsync(id);
       
            TempData["SuccessMessage"] = "Xóa s?n ph?m thành công!";
            return RedirectToAction(nameof(Index));
        }


        private async Task<string?> UploadImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine("wwwroot", "uploads", "products", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/uploads/products/" + fileName;
        }


        private async Task LoadDropdowns()
        {
            ViewBag.Categories = await _adminProductService.GetCategoriesAsync();
            ViewBag.Brands = await _adminProductService.GetBrandsAsync();
        }
    }
}
