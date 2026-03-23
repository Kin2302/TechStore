using Application.Interfaces.Admin;
using TechStore.Domain.Enums;
using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace TechStore.Infrastructure.Plugins
{
    /// <summary>
    /// Plugin quan ly don hang va nguoi dung cho Admin
    /// </summary>
    public class AdminOrderPlugin
    {
        private readonly IAdminOrderService _adminOrderService;
        private readonly IUserService _userService;

        public AdminOrderPlugin(IAdminOrderService adminOrderService, IUserService userService)
        {
            _adminOrderService = adminOrderService;
            _userService = userService;
        }

        [KernelFunction("update_order_status")]
        [Description("Cap nhat trang thai don hang. VD: 'danh dau don #1204 la da giao', 'xac nhan don hang so 5'. Trang thai hop le: Pending, Confirmed, Shipping, Completed, Cancelled, Refunded.")]
        public async Task<string> UpdateOrderStatusAsync(
            [Description("Ma don hang (ID)")]
            int orderId,

            [Description("Trang thai moi: Pending, Confirmed, Shipping, Completed, Cancelled, Refunded")]
            string newStatus)
        {
            if (!Enum.TryParse<OrderStatus>(newStatus, true, out var status))
            {
                return System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    message = $"Trang thai '{newStatus}' khong hop le. Cac trang thai hop le: Pending, Confirmed, Shipping, Completed, Cancelled, Refunded"
                });
            }

            var success = await _adminOrderService.UpdateOrderStatusAsync(orderId, status);

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                success = success,
                message = success
                    ? $"Da cap nhat don hang #{orderId} sang trang thai '{newStatus}'"
                    : $"Khong the cap nhat don hang #{orderId}. Kiem tra lai ma don hang hoac trang thai hien tai."
            });
        }

        [KernelFunction("search_users")]
        [Description("Tim kiem thong tin khach hang theo email hoac ten. VD: 'tim khach hang email abc@gmail.com', 'tra cuu khach hang ten Nguyen Van A'.")]
        public async Task<string> SearchUsersAsync(
            [Description("Email hoac ten khach hang can tim")]
            string query)
        {
            var users = await _userService.SearchUsersAsync(query);

            if (!users.Any())
            {
                return System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = true,
                    message = $"Khong tim thay khach hang nao voi tu khoa '{query}'",
                    count = 0,
                    users = Array.Empty<object>()
                });
            }

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                success = true,
                count = users.Count,
                users = users.Select(u => new
                {
                    userId = u.UserId,
                    email = u.Email,
                    userName = u.UserName,
                    totalOrders = u.TotalOrders,
                    totalSpent = u.TotalSpent
                }),
                message = $"Tim thay {users.Count} khach hang"
            });
        }
    }
}
