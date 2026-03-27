using Microsoft.AspNetCore.Mvc;
using TechStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using TechStore.Domain.Entities;

namespace WebApp.Controllers
{
    [Route("[controller]")]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CouponController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Validate")]
        public async Task<IActionResult> Validate(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Json(new { success = false, message = "Mã r?ng" });
            }

            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == code && c.IsActive && !c.IsDeleted);
            if (coupon == null)
            {
                return Json(new { success = false, message = "Mã không t?n t?i ho?c ?ã b? t?t" });
            }

            var now = DateTime.UtcNow;
            if (coupon.StartDate.HasValue && coupon.StartDate.Value > now)
                return Json(new { success = false, message = "Mã ch?a có hi?u l?c" });
            if (coupon.EndDate.HasValue && coupon.EndDate.Value < now)
                return Json(new { success = false, message = "Mã ?ã h?t h?n" });
            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
                return Json(new { success = false, message = "Mã ?ã ??t gi?i h?n s? d?ng" });

            // compute discounted total based on current cart stored in session or cookie - simplest: compute from Cart cookie via endpoint
            // For now return success with basic info
            return Json(new
            {
                success = true,
                message = coupon.IsPercent ? $"Áp d?ng: {coupon.Amount}%" : $"Áp d?ng: {coupon.Amount:N0}?",
                coupon = new { coupon.Code, coupon.IsPercent, coupon.Amount }
            });
        }
    }
}
