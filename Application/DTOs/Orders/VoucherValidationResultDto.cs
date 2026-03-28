namespace Application.DTOs.Orders
{
    public class VoucherValidationResultDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Code { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}