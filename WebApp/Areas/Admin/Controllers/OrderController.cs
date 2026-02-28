using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TechStore.Domain.Enums;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly IAdminOrderService _adminOrderService;

        public OrderController(IAdminOrderService adminOrderService)
        {
            _adminOrderService = adminOrderService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? status)
        {
            OrderStatus? statusFilter = null;
            if (Enum.TryParse<OrderStatus>(status, true, out var parsed))
            {
                statusFilter = parsed;
            }
            var orders = await _adminOrderService.GetAllOrdersAsync(statusFilter);
            ViewBag.CurrentStatus = status;
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _adminOrderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            Enum.TryParse<OrderStatus>(order.Status, out var currentStatus);
            ViewBag.ValidStatuses = _adminOrderService.GetValidNextStatuses(currentStatus);
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus)
        {
            var order = await _adminOrderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            if (!Enum.TryParse(newStatus, true, out OrderStatus parsedStatus))
            {
                return BadRequest("Invalid status value.");
            }
            var result = await _adminOrderService.UpdateOrderStatusAsync(id, parsedStatus);
            if (!result)
            {
                TempData["Error"] = "Không thể cập nhật trạng thái đơn hàng.";
            }
            else
            {
                TempData["Success"] = "Cập nhật trạng thái thành công!";
            }
            return RedirectToAction("Details", new { id });
        }
    }
}

