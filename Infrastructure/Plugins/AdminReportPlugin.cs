using Application.Interfaces.Admin;
using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace TechStore.Infrastructure.Plugins
{
    /// <summary>
    /// Plugin bao cao cho Admin - doanh thu, thong ke don hang, top san pham
    /// </summary>
    public class AdminReportPlugin
    {
        private readonly IDashboardService _dashboardService;

        public AdminReportPlugin(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [KernelFunction("get_revenue_report")]
        [Description("Lay bao cao doanh thu trong khoang thoi gian cu the. VD: 'doanh thu hom nay', 'doanh thu thang nay', 'doanh thu tu 01/01 den 31/01'.")]
        public async Task<string> GetRevenueReportAsync(
            [Description("Ngay bat dau (format: yyyy-MM-dd). VD: '2024-01-01'")]
            string startDateStr,

            [Description("Ngay ket thuc (format: yyyy-MM-dd). VD: '2024-01-31'. De trong se lay den ngay hom nay.")]
            string? endDateStr = null)
        {
            DateTime startDate;
            DateTime endDate;

            if (!DateTime.TryParse(startDateStr, out startDate))
                startDate = DateTime.Today;

            if (string.IsNullOrEmpty(endDateStr) || !DateTime.TryParse(endDateStr, out endDate))
                endDate = DateTime.Today;

            var report = await _dashboardService.GetRevenueReportAsync(startDate, endDate);

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                success = true,
                report = new
                {
                    startDate = report.StartDate.ToString("dd/MM/yyyy"),
                    endDate = report.EndDate.ToString("dd/MM/yyyy"),
                    totalRevenue = report.TotalRevenue,
                    totalOrders = report.TotalOrders,
                    completedOrders = report.CompletedOrders,
                    cancelledOrders = report.CancelledOrders
                },
                message = $"Doanh thu tu {report.StartDate:dd/MM/yyyy} den {report.EndDate:dd/MM/yyyy}: {report.TotalRevenue:N0} VND ({report.TotalOrders} don hang)"
            });
        }

        [KernelFunction("get_best_selling_products")]
        [Description("Lay danh sach san pham ban chay nhat. VD: 'top 5 san pham ban chay', 'san pham nao ban nhieu nhat'.")]
        public async Task<string> GetBestSellingProductsAsync(
            [Description("So luong san pham can lay (mac dinh 5)")]
            int limit = 5)
        {
            var products = await _dashboardService.GetBestSellingProductsAsync(limit);

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                success = true,
                count = products.Count,
                products = products.Select(p => new
                {
                    productId = p.ProductId,
                    name = p.ProductName,
                    totalSold = p.TotalSold,
                    revenue = p.Revenue
                }),
                message = $"Top {products.Count} san pham ban chay nhat"
            });
        }

        [KernelFunction("get_order_statistics")]
        [Description("Lay thong ke so luong don hang theo trang thai (cho xu ly, dang giao, hoan thanh, da huy...). VD: 'bao nhieu don dang cho xu ly', 'thong ke don hang'.")]
        public async Task<string> GetOrderStatisticsAsync()
        {
            var stats = await _dashboardService.GetOrderStatisticsAsync();

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                success = true,
                statistics = new
                {
                    pending = stats.PendingCount,
                    confirmed = stats.ConfirmedCount,
                    shipping = stats.ShippingCount,
                    completed = stats.CompletedCount,
                    cancelled = stats.CancelledCount,
                    refunded = stats.RefundedCount,
                    total = stats.TotalCount
                },
                message = $"Tong {stats.TotalCount} don hang: {stats.PendingCount} cho xu ly, {stats.ShippingCount} dang giao, {stats.CompletedCount} hoan thanh, {stats.CancelledCount} da huy"
            });
        }
    }
}
