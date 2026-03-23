using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechStore.Domain.Enums;
using TechStore.Infrastructure.Data;


namespace TechStore.Infrastructure.Services
{
    public class AdminOrderService : IAdminOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminOrderService> _logger;
        private readonly IGHNService _ghnService;

        public AdminOrderService(
            ApplicationDbContext context,
            ILogger<AdminOrderService> logger,
            IGHNService ghnService)
        {
            _context = context;
            _logger = logger;
            _ghnService = ghnService;
        }

        public async Task<List<OrderDto>> GetAllOrdersAsync(OrderStatus? status = null)
        {
            var ordersFromDb = await _context.Orders
                .Where(o => status == null || o.Status == status)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Images)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return ordersFromDb.Select(MapOrderDto).ToList();
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            return order == null ? null : MapOrderDto(order);
        }

        public List<OrderStatus> GetValidNextStatuses(OrderStatus current)
        {
            return current switch
            {
                OrderStatus.Pending => new List<OrderStatus> { OrderStatus.Confirmed, OrderStatus.Cancelled },
                OrderStatus.Confirmed => new List<OrderStatus> { OrderStatus.Shipping, OrderStatus.Cancelled },
                OrderStatus.Shipping => new List<OrderStatus> { OrderStatus.Completed },
                _ => new List<OrderStatus>()
            };
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return false;

            var validNextStatuses = GetValidNextStatuses(order.Status);
            if (!validNextStatuses.Contains(newStatus)) return false;

            order.Status = newStatus;

            if (newStatus == OrderStatus.Cancelled)
            {
                var productIds = order.OrderDetails.Select(od => od.ProductId).ToList();
                var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

                foreach (var item in order.OrderDetails)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product != null) product.Stock += item.Quantity;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SyncShippingStatusFromGhnAsync(int orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null || string.IsNullOrWhiteSpace(order.ShippingCode)) return false;

            var detail = await _ghnService.GetShipmentDetailAsync(order.ShippingCode);
            if (detail == null || string.IsNullOrWhiteSpace(detail.Status)) return false;

            order.ShippingStatusRaw = detail.Status;
            order.Status = MapGhnStatus(detail.Status, order.Status);

            await _context.SaveChangesAsync();
            return true;
        }

        private static OrderStatus MapGhnStatus(string ghnStatus, OrderStatus currentStatus)
        {
            var s = ghnStatus.Trim().ToLowerInvariant();

            if (s is "delivered")
                return OrderStatus.Completed;

            if (s is "cancel" or "returned" or "return" or "delivery_fail" or "damage" or "lost")
                return OrderStatus.Cancelled;

            if (s is "delivering" or "transporting" or "sorting")
                return OrderStatus.Shipping;

            if (s is "ready_to_pick" or "picking" or "picked" or "storing")
                return OrderStatus.Confirmed;

            return currentStatus;
        }

        private static OrderDto MapOrderDto(TechStore.Domain.Entities.Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                FullName = order.FullName,
                PhoneNumber = order.PhoneNumber,
                ShippingAddress = order.ShippingAddress,
                ShippingFee = order.ShippingFee,
                ShippingProvider = order.ShippingProvider,
                ShippingCode = order.ShippingCode,
                ShippingStatusRaw = order.ShippingStatusRaw,
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod,
                Status = order.Status.ToString(),
                OrderDate = order.OrderDate,
                Note = order.Note,
                Items = order.OrderDetails.Select(od => new OrderItemDto
                {
                    ProductId = od.ProductId,
                    ProductName = od.Product.Name,
                    Quantity = od.Quantity,
                    Price = od.Price,
                    ImageUrl = od.Product.Images.FirstOrDefault()?.ImageUrl
                }).ToList()
            };
        }
    }
}

 
