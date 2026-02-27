using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Email { get; set; } = "";
        public string ShippingAddress { get; set; } = "";

        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string Status { get; set; }

        public DateTime OrderDate { get; set; }

        public string? Note { get; set; }

        public List<OrderItemDto> Items { get; set; } = new();

        public string StatusDisplay => Status switch
        {
            "Pending" => "Chờ xác nhận",
            "Confirmed" => "Đã xác nhận",
            "Shipping" => "Đang giao hàng",
            "Completed" => "Hoàn thành",
            "Cancelled" => "Đã hủy",
            "Refunded" => "Đã hoàn tiền",
            _ => Status  // default
        };

        public string PaymentMethodDisplay => PaymentMethod switch
        {
            "COD" => "Thanh toán khi nhận hàng",
            "BankTransfer" => "Chuyển khoản ngân hàng",
            _ => PaymentMethod
        };

        public string StatusBadgeClass => Status switch
        {
            "Pending" => "bg-warning text-dark",
            "Confirmed" => "bg-info",
            "Shipping" => "bg-primary",
            "Completed" => "bg-success",
            "Cancelled" => "bg-danger",
            "Refunded" => "bg-secondary",
            _ => "bg-secondary"
        };
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
        public string? ImageUrl { get; set; }
    }

    public class CreateOrderResult
    {
        public bool Success { get; set; }
        public int? OrderId { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> OutOfStockProducts { get; set; } = new();
    }
}
