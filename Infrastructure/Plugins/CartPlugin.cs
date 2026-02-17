using System.ComponentModel;
using Application.Interfaces;
using Microsoft.SemanticKernel;

namespace TechStore.Infrastructure.Plugins
{
    /// <summary>
    /// Plugin cho AI Agent để quản lý giỏ hàng
    /// </summary>
    public class CartPlugin
    {
        private readonly ICartService _cartService;

        public CartPlugin(ICartService cartService)
        {
            _cartService = cartService;
        }

        [KernelFunction("add_to_cart")]
        [Description("Thêm sản phẩm vào giỏ hàng. Trả về thông tin giỏ hàng sau khi thêm.")]
        public async Task<string> AddToCartAsync(
            [Description("ID của sản phẩm cần thêm vào giỏ")] 
            int productId,
            
            [Description("Số lượng cần thêm (mặc định là 1)")] 
            int quantity = 1)
        {
            try
            {
                // Lấy thông tin sản phẩm
                var productInfo = await _cartService.GetProductForCartAsync(productId);
                
                if (productInfo == null)
                {
                    return System.Text.Json.JsonSerializer.Serialize(new
                    {
                        success = false,
                        message = $"❌ Không tìm thấy sản phẩm với ID {productId}"
                    });
                }

                // Lấy giỏ hàng hiện tại
                var cart = _cartService.GetCart();
                
                // Kiểm tra sản phẩm đã có trong giỏ chưa
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

                // Lưu giỏ hàng
                _cartService.SaveCart(cart);

                var cartTotal = _cartService.GetCartTotal();
                var cartCount = _cartService.GetCartCount();

                return System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = true,
                    message = $"✅ Đã thêm {quantity}x {productInfo.ProductName} vào giỏ hàng!",
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
                    message = $"❌ Lỗi: {ex.Message}"
                });
            }
        }

        [KernelFunction("remove_from_cart")]
        [Description("Xóa sản phẩm khỏi giỏ hàng")]
        public Task<string> RemoveFromCartAsync(
            [Description("ID của sản phẩm cần xóa")] 
            int productId)
        {
            var cart = _cartService.GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item == null)
            {
                return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "❌ Sản phẩm không có trong giỏ hàng"
                }));
            }

            cart.Remove(item);
            _cartService.SaveCart(cart);

            return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(new
            {
                success = true,
                message = $"✅ Đã xóa {item.ProductName} khỏi giỏ hàng",
                cartCount = _cartService.GetCartCount()
            }));
        }

        [KernelFunction("view_cart")]
        [Description("Xem nội dung giỏ hàng hiện tại")]
        public Task<string> GetCartAsync()
        {
            var cart = _cartService.GetCart();

            if (!cart.Any())
            {
                return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = true,
                    message = "🛒 Giỏ hàng trống",
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
                message = $"🛒 Giỏ hàng có {cart.Count} sản phẩm",
                items = items,
                total = _cartService.GetCartTotal(),
                count = _cartService.GetCartCount()
            }));
        }

        [KernelFunction("clear_cart")]
        [Description("Xóa toàn bộ giỏ hàng")]
        public Task<string> ClearCartAsync()
        {
            _cartService.ClearCart();

            return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(new
            {
                success = true,
                message = "✅ Đã xóa toàn bộ giỏ hàng"
            }));
        }

        [KernelFunction("update_cart_quantity")]
        [Description("Cập nhật số lượng sản phẩm trong giỏ hàng")]
        public Task<string> UpdateCartQuantityAsync(
            [Description("ID của sản phẩm")] 
            int productId,
            
            [Description("Số lượng mới")] 
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
                    message = "❌ Sản phẩm không có trong giỏ hàng"
                }));
            }

            item.Quantity = newQuantity;
            _cartService.SaveCart(cart);

            return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(new
            {
                success = true,
                message = $"✅ Đã cập nhật số lượng {item.ProductName} thành {newQuantity}",
                newTotal = item.Total,
                cartTotal = _cartService.GetCartTotal()
            }));
        }
    }
}