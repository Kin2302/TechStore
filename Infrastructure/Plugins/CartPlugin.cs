using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;
using System.ComponentModel;
using Application.Interfaces;
using Microsoft.SemanticKernel;

namespace TechStore.Infrastructure.Plugins
{
    /// <summary>
    /// Plugin cho AI Agent d? qu?n lý gi? hàng
    /// </summary>
    public class CartPlugin
    {
        private readonly ICartService _cartService;

        public CartPlugin(ICartService cartService)
        {
            _cartService = cartService;
        }

        [KernelFunction("add_to_cart")]
        [Description("Thêm s?n ph?m vào gi? hàng. Tr? v? thông tin gi? hàng sau khi thêm.")]
        public async Task<string> AddToCartAsync(
            [Description("ID c?a s?n ph?m c?n thêm vào gi?")] 
            int productId,
            
            [Description("S? lu?ng c?n thêm (m?c d?nh là 1)")] 
            int quantity = 1)
        {
            try
            {
                // L?y thông tin s?n ph?m
                var productInfo = await _cartService.GetProductForCartAsync(productId);
                
                if (productInfo == null)
                {
                    return System.Text.Json.JsonSerializer.Serialize(new
                    {
                        success = false,
                        message = $"? Không tìm th?y s?n ph?m v?i ID {productId}"
                    });
                }

                // L?y gi? hàng hi?n t?i
                var cart = _cartService.GetCart();
                
                // Ki?m tra s?n ph?m dã có trong gi? chua
                var existingItem = cart.FirstOrDefault(x => x.ProductId == productId);
                
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    productInfo.Quantity = quantity;
                    cart.Add(productInfo);
                }

                // Luu gi? hàng
                _cartService.SaveCart(cart);

                var cartTotal = _cartService.GetCartTotal();
                var cartCount = _cartService.GetCartCount();

                return System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = true,
                    message = $"? Ðã thêm {quantity}x {productInfo.ProductName} vào gi? hàng!",
                    productName = productInfo.ProductName,
                    productPrice = productInfo.Price,
                    quantity = quantity,
                    itemTotal = productInfo.Price * quantity,
                    cartCount = cartCount,
                    cartTotal = cartTotal
                });
            }
            catch (Exception ex)
            {
                return System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    message = $"? L?i: {ex.Message}"
                });
            }
        }

        [KernelFunction("remove_from_cart")]
        [Description("Xóa s?n ph?m kh?i gi? hàng")]
        public Task<string> RemoveFromCartAsync(
            [Description("ID c?a s?n ph?m c?n xóa")] 
            int productId)
        {
            var cart = _cartService.GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item == null)
            {
                return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "? S?n ph?m không có trong gi? hàng"
                }));
            }

            cart.Remove(item);
            _cartService.SaveCart(cart);

            return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(new
            {
                success = true,
                message = $"? Ðã xóa {item.ProductName} kh?i gi? hàng",
                cartCount = _cartService.GetCartCount()
            }));
        }

        [KernelFunction("view_cart")]
        [Description("Xem n?i dung gi? hàng hi?n t?i")]
        public Task<string> GetCartAsync()
        {
            var cart = _cartService.GetCart();

            if (!cart.Any())
            {
                return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = true,
                    message = "?? Gi? hàng tr?ng",
                    items = Array.Empty<object>(),
                    total = 0,
                    count = 0
                }));
            }

            var items = cart.Select(item => new
            {
                productId = item.ProductId,
                name = item.ProductName,
                price = item.Price,
                quantity = item.Quantity,
                total = item.Total
            }).ToList();

            return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(new
            {
                success = true,
                message = $"?? Gi? hàng có {cart.Count} s?n ph?m",
                items = items,
                total = _cartService.GetCartTotal(),
                count = _cartService.GetCartCount()
            }));
        }

        [KernelFunction("clear_cart")]
        [Description("Xóa toàn b? gi? hàng")]
        public Task<string> ClearCartAsync()
        {
            _cartService.ClearCart();

            return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(new
            {
                success = true,
                message = "? Ðã xóa toàn b? gi? hàng"
            }));
        }

        [KernelFunction("update_cart_quantity")]
        [Description("C?p nh?t s? lu?ng s?n ph?m trong gi? hàng")]
        public Task<string> UpdateCartQuantityAsync(
            [Description("ID c?a s?n ph?m")] 
            int productId,
            
            [Description("S? lu?ng m?i")] 
            int newQuantity)
        {
            if (newQuantity <= 0)
            {
                return RemoveFromCartAsync(productId);
            }

            var cart = _cartService.GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item == null)
            {
                return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "? S?n ph?m không có trong gi? hàng"
                }));
            }

            item.Quantity = newQuantity;
            _cartService.SaveCart(cart);

            return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(new
            {
                success = true,
                message = $"? Ðã c?p nh?t s? lu?ng {item.ProductName} thành {newQuantity}",
                newTotal = item.Total,
                cartTotal = _cartService.GetCartTotal()
            }));
        }
    }
}