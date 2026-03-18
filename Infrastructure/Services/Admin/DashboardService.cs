using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using TechStore.Domain.Enums;
using TechStore.Infrastructure.Data;

namespace TechStore.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            var today = DateTime.UtcNow.Date;

            // Stat cards
            var totalOrders = await _context.Orders.CountAsync();
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
            var confirmedOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Confirmed);
            var shippingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Shipping);
            var completedOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Completed);
            var cancelledOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Cancelled);

            var totalRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            var todayRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed && o.OrderDate.Date == today)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            var totalProducts = await _context.Products.CountAsync(p => !p.IsDeleted);
            var lowStockProducts = await _context.Products.CountAsync(p => !p.IsDeleted && p.Stock <= 5 && p.Stock > 0);

            // Ðon hàng g?n dây (5 don m?i nh?t)
            var recentOrdersDb = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Images)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            var recentOrders = recentOrdersDb.Select(o => new OrderDto
            {
                Id = o.Id,
                FullName = o.FullName,
                PhoneNumber = o.PhoneNumber,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),
                OrderDate = o.OrderDate,
                PaymentMethod = o.PaymentMethod,
                Items = o.OrderDetails.Select(od => new OrderItemDto
                {
                    ProductId = od.ProductId,
                    ProductName = od.Product.Name,
                    Quantity = od.Quantity,
                    Price = od.Price,
                    ImageUrl = od.Product.Images.FirstOrDefault()?.ImageUrl
                }).ToList()
            }).ToList();

            // SP bán ch?y (top 5 t? OrderDetail dã Completed)
            var topProducts = await _context.OrderDetails
                .Where(od => od.Order.Status == OrderStatus.Completed)
                .GroupBy(od => new { od.ProductId, od.Product.Name })
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalSold = g.Sum(od => od.Quantity),
                    Revenue = g.Sum(od => od.Price * od.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToListAsync();

            // L?y ?nh cho top products
            var topProductIds = topProducts.Select(p => p.ProductId).ToList();
            var productImages = await _context.ProductImages
                .Where(pi => topProductIds.Contains(pi.ProductId))
                .GroupBy(pi => pi.ProductId)
                .Select(g => new { ProductId = g.Key, ImageUrl = g.FirstOrDefault()!.ImageUrl })
                .ToListAsync();

            foreach (var tp in topProducts)
            {
                tp.ImageUrl = productImages.FirstOrDefault(pi => pi.ProductId == tp.ProductId)?.ImageUrl;
            }

            return new DashboardDto
            {
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                ConfirmedOrders = confirmedOrders,
                ShippingOrders = shippingOrders,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                TotalRevenue = totalRevenue,
                TodayRevenue = todayRevenue,
                TotalProducts = totalProducts,
                LowStockProducts = lowStockProducts,
                RecentOrders = recentOrders,
                TopProducts = topProducts
            };
        }
    }
}