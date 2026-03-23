using Application.DTOs.Admin;
using Application.DTOs.Catalog;

namespace Application.Interfaces.Admin
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync();
        Task<RevenueReportDto> GetRevenueReportAsync(DateTime startDate, DateTime endDate);
        Task<List<TopProductDto>> GetBestSellingProductsAsync(int limit);
        Task<OrderStatisticsDto> GetOrderStatisticsAsync();
    }
}
