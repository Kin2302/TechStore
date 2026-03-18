using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;
using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechStore.Domain.Enums;

namespace Application.Interfaces.Admin {
    public interface IAdminOrderService
    {
        Task<List<OrderDto>> GetAllOrdersAsync(OrderStatus? status = null);
        Task<OrderDto?> GetOrderByIdAsync(int orderId);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
        List<OrderStatus> GetValidNextStatuses(OrderStatus current);
    }
}
