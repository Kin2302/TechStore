using Application.DTOs.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class BrandController : Controller
    {
        private readonly IAdminBrandService _brandService;

        public BrandController(IAdminBrandService brandService)
        {
            _brandService = brandService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var brands = await _brandService.GetAllAsync();
            return View(brands);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View(new BrandDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(BrandDto model, IFormFile? logoFile)
        {
            if (!ModelState.IsValid)
                return View(model);

            var logoUrl = await UploadLogoAsync(logoFile);
            await _brandService.CreateAsync(model, logoUrl);
            TempData["SuccessMessage"] = "Thêm thuong hi?u thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _brandService.GetByIdAsync(id);
            if (brand == null) return NotFound();
            return View(brand);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BrandDto model, IFormFile? logoFile)
        {
            if (!ModelState.IsValid)
                return View(model);

            var logoUrl = await UploadLogoAsync(logoFile);
            await _brandService.UpdateAsync(model, logoUrl);
            TempData["SuccessMessage"] = "C?p nh?t thuong hi?u thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _brandService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Xóa thuong hi?u thành công!";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string?> UploadLogoAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine("wwwroot", "uploads", "brands", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/uploads/brands/" + fileName;
        }
    }
}