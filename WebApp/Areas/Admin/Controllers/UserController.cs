using Application.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(string userId, string role)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == userId && role == "User")
            {
                TempData["Error"] = "Không thể tự hạ quyền tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Index));
            }

            var ok = await _userService.UpdateUserRoleAsync(userId, role);
            if (ok)
            {
                TempData["Success"] = "Cập nhật quyền thành công.";
            }
            else
            {
                TempData["Error"] = "Không thể cập nhật quyền cho tài khoản này.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
