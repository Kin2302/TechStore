using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
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

        public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime startDate, DateTime endDate)
        {
            var orders = _context.Orders
                .Where(o => o.OrderDate.Date >= startDate.Date && o.OrderDate.Date <= endDate.Date);

            var totalOrders = await orders.CountAsync();
            var completedOrders = await orders.CountAsync(o => o.Status == OrderStatus.Completed);
            var cancelledOrders = await orders.CountAsync(o => o.Status == OrderStatus.Cancelled);
            var totalRevenue = await orders
                .Where(o => o.Status == OrderStatus.Completed)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            return new RevenueReportDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public async Task<List<TopProductDto>> GetBestSellingProductsAsync(int limit)
        {
            return await _context.OrderDetails
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
                .Take(limit)
                .ToListAsync();
        }

        public async Task<OrderStatisticsDto> GetOrderStatisticsAsync()
        {
            return new OrderStatisticsDto
            {
                PendingCount = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
                ConfirmedCount = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Confirmed),
                ShippingCount = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Shipping),
                CompletedCount = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Completed),
                CancelledCount = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Cancelled),
                RefundedCount = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Refunded),
                TotalCount = await _context.Orders.CountAsync()
            };
        }
    }
}
