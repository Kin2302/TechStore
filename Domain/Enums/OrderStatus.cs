using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechStore.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 0,        // Chờ xác nhận
        Confirmed = 1,      // Đã xác nhận
        Shipping = 2,       // Đang giao
        Completed = 3,      // Hoàn thành
        Cancelled = 4,      // Đã hủy
        Refunded = 5        // Hoàn tiền (nếu cần)
    }
}
