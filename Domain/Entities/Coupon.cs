using System;
using Domain.Common;

namespace TechStore.Domain.Entities
{
    public class Coupon : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        // If true, Amount is percentage (0-100). If false, Amount is fixed money (VND).
        public bool IsPercent { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
