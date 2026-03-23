using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechStore.Infrastructure.Data;
using TechStore.Domain.Entities;
using TechStore.Domain.Enums;

namespace TechStore.Infrastructure.Services
{
    public class OrderService : IOrderService
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderService> logger;
        private readonly IGHNService _ghnService;

        public OrderService(ApplicationDbContext context, ILogger<OrderService> logger, IGHNService ghnService)
        {
            _context = context;
            this.logger = logger;
            _ghnService = ghnService;
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
                    ErrorMessage = "M?t s? s?n ph?m không d? hàng",
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
                    ShippingFee = checkout.ShippingFee, // NEW
                    TotalAmount = cartItems.Sum(ci => ci.Total) + checkout.ShippingFee,
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

                try
                {
                    if (checkout.DistrictId > 0 && !string.IsNullOrWhiteSpace(checkout.WardCode))
                    {
                        var shipment = await _ghnService.CreateShipmentAsync(new GHNCreateShipmentRequestDto
                        {
                            ClientOrderCode = order.Id.ToString(),
                            ToName = checkout.FullName,
                            ToPhone = checkout.PhoneNumber,
                            ToAddress = checkout.ShippingAddress,
                            ToDistrictId = checkout.DistrictId,
                            ToWardCode = checkout.WardCode,
                            InsuranceValue = (int)Math.Round(order.TotalAmount, 0),
                            CodAmount = 0,
                            Note = checkout.Note,
                            Items = cartItems.Select(x => new GHNShipmentItemDto
                            {
                                Name = x.ProductName,
                                Quantity = x.Quantity,
                                Price = (int)Math.Round(x.Price, 0)
                            }).ToList()
                        });

                        if (!string.IsNullOrWhiteSpace(shipment?.OrderCode))
                        {
                            order.ShippingProvider = "GHN";
                            order.ShippingCode = shipment.OrderCode;
                            order.ShippingStatusRaw = "ready_to_pick";
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Create GHN shipment failed for order {OrderId}", order.Id);
                }

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
                    ErrorMessage = "Ðã x?y ra l?i khi t?o don hàng"
                };
            }

        }

        public async Task<OrderDto?> GetOrderByIdAsync(int orderId, string userId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null) return null;

            return new OrderDto
            {
                Id = order.Id,
                FullName = order.FullName,
                PhoneNumber = order.PhoneNumber,
                ShippingAddress = order.ShippingAddress,
                ShippingFee = order.ShippingFee,
                ShippingProvider = order.ShippingProvider,   // ADD
                ShippingCode = order.ShippingCode,           // ADD
                ShippingStatusRaw = order.ShippingStatusRaw, // ADD
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
                .ToListAsync();

            return ordersFromDb.Select(order => new OrderDto
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
            }).ToList();
        }

        public async Task<List<string>> ValidateStockAsync(List<CartItemDto> cartItems)
        {
            var productIds = cartItems.Select(ci => ci.ProductId).ToList();
            var productsInDb = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Stock);

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
                    logger.LogInformation(
                        "Product {ProductId} has insufficient stock. Requested: {Requested}, Available: {Available}",
                        item.ProductId, item.Quantity, stock);
                    outOfStockMessages.Add($"Sản phẩm {item.ProductName} chỉ còn {stock} (bạn đặt {item.Quantity})");
                }
            }

            return outOfStockMessages;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return false;
            }

            order.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
