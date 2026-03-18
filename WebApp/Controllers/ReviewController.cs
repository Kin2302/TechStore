using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;

namespace WebApp.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReviewDto model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ReviewError"] = "Dữ liệu đánh giá không hợp lệ.";
                return RedirectToAction("Detail", "Home", new { id = model.ProductId, tab = "reviews" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name ?? "Người dùng";

            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["ReviewError"] = "Không xác định được tài khoản người dùng.";
                return RedirectToAction("Detail", "Home", new { id = model.ProductId, tab = "reviews" });
            }

            var result = await _reviewService.CreateReviewAsync(userId, userName, model);

            if (result.Success)
            {
                TempData["ReviewSuccess"] = result.Message;
            }
            else
            {
                TempData["ReviewError"] = result.Message;
            }

            return RedirectToAction("Detail", "Home", new { id = model.ProductId, tab = "reviews" });
        }
    }
}