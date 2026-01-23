using Domain.Common;
using TechStore.Domain.Enums;

namespace TechStore.Domain.Entities
{
    public class Order : BaseEntity
    {
        public string UserId { get; set; } // Link tới IdentityUser
        public DateTime OrderDate { get; set; } = DateTime.Now;

        // Thông tin người nhận (Lưu cứng tại thời điểm đặt)
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string ShippingAddress { get; set; }
        public string? Note { get; set; }

        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}