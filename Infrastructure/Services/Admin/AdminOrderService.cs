using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechStore.Domain.Enums;
using TechStore.Infrastructure.Data;

namespace TechStore.Infrastructure.Services
{
    public class AdminOrderService : IAdminOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminOrderService> _logger;

        public AdminOrderService(ApplicationDbContext context, ILogger<AdminOrderService> logger)
        {
            _context = context;
            _logger = logger;
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

            return ordersFromDb.Select(order => new OrderDto
            {
                Id = order.Id,
                FullName = order.FullName,
                PhoneNumber = order.PhoneNumber,
                ShippingAddress = order.ShippingAddress,
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
            }).ToList();



        }

        public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders
                            .Include(o => o.OrderDetails)
                            .ThenInclude(od => od.Product)
                            .ThenInclude(p => p.Images)  // 
                            .FirstOrDefaultAsync(o => o.Id == orderId );

            if (order == null) return null;

            return new OrderDto
            {
                Id = order.Id,
                FullName = order.FullName,
                PhoneNumber = order.PhoneNumber,
                ShippingAddress = order.ShippingAddress,
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
                _logger.LogInformation($"Order {orderId} cancelled. Initiating refund process.");
                var productIds = order.OrderDetails.Select(od => od.ProductId).ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                foreach (var item in order.OrderDetails)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product != null)
                    {
                        product.Stock += item.Quantity;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            await _context.SaveChangesAsync();
            return true;

        }
    }
}
