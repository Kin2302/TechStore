using Domain.Common;
using TechStore.Domain.Enums;

namespace TechStore.Domain.Entities
{
    public class Order : BaseEntity
    {
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;

        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string ShippingAddress { get; set; }
        public string? Note { get; set; }

        public decimal ShippingFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? VoucherCode { get; set; }

        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public string? ShippingProvider { get; set; }
        public string? ShippingCode { get; set; }
        public string? ShippingStatusRaw { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}