using Application.DTOs.Catalog;
using Application.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CouponController : Controller
    {
        private readonly IAdminCouponService _couponService;

        public CouponController(IAdminCouponService couponService)
        {
            _couponService = couponService;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _couponService.GetAllAsync();
            return View(items);
        }

        public IActionResult Create()
        {
            return View(new CouponDto { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CouponDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _couponService.CreateAsync(dto);
            TempData["SuccessMessage"] = "Đã tạo phiếu giảm giá.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _couponService.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CouponDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _couponService.UpdateAsync(dto);
            TempData["SuccessMessage"] = "Đã cập nhật phiếu giảm giá.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _couponService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Đã xóa phiếu giảm giá.";
            return RedirectToAction("Index");
        }
    }
}
