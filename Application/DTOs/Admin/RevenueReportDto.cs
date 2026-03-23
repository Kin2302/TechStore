namespace Application.DTOs.Admin
{
    public class RevenueReportDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class OrderStatisticsDto
    {
        public int PendingCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int ShippingCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        public int RefundedCount { get; set; }
        public int TotalCount { get; set; }
    }
}
