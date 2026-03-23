using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TechStore.Domain.Enums;
using Application.DTOs.Admin;
using Application.DTOs.Catalog;
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
                TempData["Error"] = "Không th? c?p nh?t tr?ng thái don hàng.";
            }
            else
            {
                TempData["Success"] = "C?p nh?t tr?ng thái thành công!";
            }
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> SyncShippingStatus(int id)
        {
            var ok = await _adminOrderService.SyncShippingStatusFromGhnAsync(id);

            if (ok)
            {
                TempData["Success"] = "Đồng bộ trạng thái GHN thành công.";
            }
            else
            {
                TempData["Error"] = "Không thể đồng bộ trạng thái GHN.";
            }

            return RedirectToAction("Details", new { id });
        }
    }
}

