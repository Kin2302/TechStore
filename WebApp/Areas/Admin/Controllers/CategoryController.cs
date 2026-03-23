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
    public class CategoryController : Controller
    {
        private readonly IAdminCategoryService _categoryService;

        public CategoryController(IAdminCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllAsync();
            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadParentCategories();
            return View(new CategoryDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryDto model)
        {
            if (!ModelState.IsValid)
            {
                await LoadParentCategories();
                return View(model);
            }

            await _categoryService.CreateAsync(model);
            TempData["SuccessMessage"] = "Thêm danh m?c thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null) return NotFound();

            await LoadParentCategories(id);
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CategoryDto model)
        {
            if (!ModelState.IsValid)
            {
                await LoadParentCategories(model.Id);
                return View(model);
            }

            await _categoryService.UpdateAsync(model);
            TempData["SuccessMessage"] = "C?p nh?t danh m?c thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _categoryService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Xóa danh m?c thành công!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadParentCategories(int? excludeId = null)
        {
            var parents = await _categoryService.GetParentCategoriesAsync();
            ViewBag.ParentCategories = excludeId.HasValue
                ? parents.Where(c => c.Id != excludeId)
                : parents;
        }
    }
}