using Domain.Common;
using TechStore.Domain.Enums;

namespace TechStore.Domain.Entities
{
    public class Voucher : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }

        public VoucherType Type { get; set; } = VoucherType.FixedAmount;
        public decimal Value { get; set; }

        public decimal MinOrderAmount { get; set; } = 0;
        public decimal? MaxDiscountAmount { get; set; }

        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

        public bool IsActive { get; set; } = true;

        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; }

        public int? MaxUsagePerUser { get; set; }

        public ICollection<VoucherUsage> Usages { get; set; } = new List<VoucherUsage>();
    }
}