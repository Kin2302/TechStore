using Application.DTOs.Admin;
using Application.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStore.Domain.Enums;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class VoucherController : Controller
    {
        private readonly IAdminVoucherService _voucherService;

        public VoucherController(IAdminVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vouchers = await _voucherService.GetAllAsync();
            return View(vouchers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new AdminVoucherDto
            {
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddDays(7),
                IsActive = true,
                Type = VoucherType.FixedAmount
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(AdminVoucherDto model)
        {
            ValidateBusinessRules(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _voucherService.CreateAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(nameof(model.Code), result.ErrorMessage ?? "Không thể tạo voucher.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Tạo voucher thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var voucher = await _voucherService.GetByIdAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }

            return View(voucher);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AdminVoucherDto model)
        {
            ValidateBusinessRules(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _voucherService.UpdateAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(nameof(model.Code), result.ErrorMessage ?? "Không thể cập nhật voucher.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Cập nhật voucher thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Activate(int id)
        {
            var ok = await _voucherService.SetActiveAsync(id, true);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "Đã kích hoạt voucher."
                : "Không tìm thấy voucher.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(int id)
        {
            var ok = await _voucherService.SetActiveAsync(id, false);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "Đã tắt voucher."
                : "Không tìm thấy voucher.";

            return RedirectToAction(nameof(Index));
        }

        private void ValidateBusinessRules(AdminVoucherDto model)
        {
            if (model.EndAt <= model.StartAt)
            {
                ModelState.AddModelError(nameof(model.EndAt), "Thời gian kết thúc phải sau thời gian bắt đầu.");
            }

            if (model.Type == VoucherType.Percentage && model.Value > 100)
            {
                ModelState.AddModelError(nameof(model.Value), "Voucher theo phần trăm không được vượt quá 100%.");
            }

            if (model.MaxUsagePerUser.HasValue &&
                model.UsageLimit.HasValue &&
                model.MaxUsagePerUser.Value > model.UsageLimit.Value)
            {
                ModelState.AddModelError(nameof(model.MaxUsagePerUser), "Giới hạn mỗi user không được lớn hơn tổng lượt dùng.");
            }
        }
    }
}