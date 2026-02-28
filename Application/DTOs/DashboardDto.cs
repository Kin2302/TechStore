using System;
using System.Collections.Generic;

namespace Application.DTOs
{
    public class DashboardDto
    {
        // Stat cards
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }

        // Đơn hàng gần đây (5 đơn mới nhất)
        public List<OrderDto> RecentOrders { get; set; } = new();

        // SP bán chạy
        public List<TopProductDto> TopProducts { get; set; } = new();

        // Thống kê theo status
        public int ConfirmedOrders { get; set; }
        public int ShippingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
    }

    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string? ImageUrl { get; set; }
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
    }
}