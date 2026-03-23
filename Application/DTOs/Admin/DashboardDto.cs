using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;
using System;
using System.Collections.Generic;

namespace Application.DTOs.Admin {
    public class DashboardDto
    {
        // Stat cards
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }

        // Ðon hàng g?n dây (5 don m?i nh?t)
        public List<OrderDto> RecentOrders { get; set; } = new();

        // SP bán ch?y
        public List<TopProductDto> TopProducts { get; set; } = new();

        // Th?ng kê theo status
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