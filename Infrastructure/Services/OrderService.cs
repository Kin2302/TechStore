using Application.DTOs;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechStore.Infrastructure.Data;
using TechStore.Domain.Entities;
using TechStore.Domain.Enums;

namespace TechStore.Infrastructure.Services
{
    public class OrderService : IOrderService
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderService> logger;

        public OrderService(ApplicationDbContext context, ILogger<OrderService> logger)
        {
            _context = context;
            this.logger = logger;
        }


        public async Task<bool> CancelOrderAsync(int orderId, string userId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)  
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null || order.Status != OrderStatus.Pending)
            {
                return false;
            }

            order.Status = OrderStatus.Cancelled;

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

        public async Task<CreateOrderResult> CreateOrderAsync(string userId, CheckoutDto checkout, List<CartItemDto> cartItems)
        {
            var outOfStockMessages = await ValidateStockAsync(cartItems);

            if (outOfStockMessages.Any())
            {
                return new CreateOrderResult
                {
                    Success = false,
                    ErrorMessage = "Một số sản phẩm không đủ hàng",
                    OutOfStockProducts = outOfStockMessages
                };
            }
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    UserId = userId,
                    FullName = checkout.FullName,
                    PhoneNumber = checkout.PhoneNumber,
                    ShippingAddress = checkout.ShippingAddress,
                    Note = checkout.Note,
                    PaymentMethod = checkout.PaymentMethod,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = cartItems.Sum(ci => ci.Total),
                    Status = OrderStatus.Pending
                };

                var orderDetails = cartItems.Select(ci => new OrderDetail
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.Price
                }).ToList();
                order.OrderDetails = orderDetails;

                var productIds = cartItems.Select(ci => ci.ProductId).ToList();
                var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

                foreach (var item in cartItems)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product != null)
                    {
                        product.Stock -= item.Quantity;
                    }
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CreateOrderResult
                {
                    Success = true,
                    OrderId = order.Id
                };

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating order for user {UserId}", userId);
                await transaction.RollbackAsync();
                return new CreateOrderResult
                {
                    Success = false,
                    ErrorMessage = "Đã xảy ra lỗi khi tạo đơn hàng"
                };
            }

        }

        public async Task<OrderDto?> GetOrderByIdAsync(int orderId, string userId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ThenInclude(p => p.Images)  // 
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

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

        public async Task<List<OrderDto>> GetUserOrdersAsync(string userId)
        {
            var ordersFromDb = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Images)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();  // ← Execute SQL query

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


        public async Task<List<string>> ValidateStockAsync(List<CartItemDto> cartItems)
        {
            var productIds = cartItems.Select(ci => ci.ProductId).ToList();
            
            
            var productsInDb =await  _context.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Stock);
            List<string> outOfStockMessages = new();

            foreach (var item in cartItems)
            {
                if (!productsInDb.TryGetValue(item.ProductId, out var stock))
                {
                    logger.LogWarning("Product with ID {ProductId} not found", item.ProductId);
                    outOfStockMessages.Add($"Sản phẩm {item.ProductName} không còn tồn tại");
                    continue;
                }
                if (item.Quantity > stock)
                {
                    logger.LogInformation("Product {ProductId} has insufficient stock. Requested: {Requested}, Available: {Available}", item.ProductId, item.Quantity, stock);
                    outOfStockMessages.Add($"Sản phẩm {item.ProductName} chỉ còn {stock} (bạn đặt {item.Quantity})");
                }
          
            }
            return outOfStockMessages;

        }



    }
}
