using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;
using Application.DTOs;
using TechStore.Domain.Enums;

namespace Application.Interfaces.Orders {
    public interface IOrderService
    {
        Task<CreateOrderResult> CreateOrderAsync(string userId, CheckoutDto checkout, List<CartItemDto> cartItems);
        Task<OrderDto?> GetOrderByIdAsync(int orderId, string userId);
        Task<List<OrderDto>> GetUserOrdersAsync(string userId);
        Task<bool> CancelOrderAsync(int orderId, string userId);
        Task<List<string>> ValidateStockAsync(List<CartItemDto> cartItems);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);
    }
}
