using Domain.Common;

namespace TechStore.Domain.Entities
{
    public class VoucherUsage : BaseEntity
    {
        public int VoucherId { get; set; }
        public Voucher Voucher { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public decimal DiscountAmount { get; set; }
        public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    }
}