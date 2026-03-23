using Application.DTOs.Orders;
using Application.Interfaces.Orders;
using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace TechStore.Infrastructure.Plugins
{
    /// <summary>
    /// Plugin cho AI Agent de xem lich su don hang va trang thai don hang
    /// </summary>
    public class OrderPlugin
    {
        private readonly IOrderService _orderService;

        public OrderPlugin(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [KernelFunction("get_order_history")]
        [Description("Lay danh sach cac don hang gan day cua khach hang. Can userId de truy van.")]
        public async Task<string> GetOrderHistoryAsync(
            [Description("ID cua nguoi dung (UserId)")]
            string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "Ban can dang nhap de xem lich su don hang."
                });
            }

            var orders = await _orderService.GetUserOrdersAsync(userId);

            if (!orders.Any())
            {
                return System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = true,
                    message = "Ban chua co don hang nao.",
                    count = 0,
                    orders = Array.Empty<object>()
                });
            }

            var result = orders.Select(o => new
            {
                id = o.Id,
                orderDate = o.OrderDate.ToString("dd/MM/yyyy HH:mm"),
                status = o.Status,
                statusDisplay = o.StatusDisplay,
                totalAmount = o.TotalAmount,
                shippingFee = o.ShippingFee,
                paymentMethod = o.PaymentMethodDisplay,
                itemCount = o.Items.Count
            }).ToList();

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                success = true,
                message = $"Ban co {result.Count} don hang",
                count = result.Count,
                orders = result
            });
        }

        [KernelFunction("get_order_status")]
        [Description("Kiem tra trang thai cua mot don hang cu the theo ma don hang (orderId). Tra ve trang thai hien tai va chi tiet don hang.")]
        public async Task<string> GetOrderStatusAsync(
            [Description("Ma don hang (ID)")]
            int orderId,

            [Description("ID cua nguoi dung (UserId)")]
            string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "Ban can dang nhap de xem trang thai don hang."
                });
            }

            var order = await _orderService.GetOrderByIdAsync(orderId, userId);

            if (order == null)
            {
                return System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    message = $"Khong tim thay don hang #{orderId}"
                });
            }

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                success = true,
                order = new
                {
                    id = order.Id,
                    orderDate = order.OrderDate.ToString("dd/MM/yyyy HH:mm"),
                    status = order.Status,
                    statusDisplay = order.StatusDisplay,
                    fullName = order.FullName,
                    phone = order.PhoneNumber,
                    address = order.ShippingAddress,
                    totalAmount = order.TotalAmount,
                    shippingFee = order.ShippingFee,
                    paymentMethod = order.PaymentMethodDisplay,
                    items = order.Items.Select(i => new
                    {
                        name = i.ProductName,
                        price = i.Price,
                        quantity = i.Quantity,
                        total = i.Total
                    })
                },
                message = $"Don hang #{orderId}: {order.StatusDisplay}"
            });
        }
    }
}
