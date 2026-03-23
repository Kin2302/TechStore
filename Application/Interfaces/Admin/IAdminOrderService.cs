using Application.DTOs.Orders;
using TechStore.Domain.Enums;

namespace Application.Interfaces.Admin
{
    public interface IAdminOrderService
    {
        Task<List<OrderDto>> GetAllOrdersAsync(OrderStatus? status = null);
        Task<OrderDto?> GetOrderByIdAsync(int orderId);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
        List<OrderStatus> GetValidNextStatuses(OrderStatus current);

        Task<bool> SyncShippingStatusFromGhnAsync(int orderId);
    }
}

