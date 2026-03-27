using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;
    using Application.DTOs;
using System.Threading.Tasks;

namespace Application.Interfaces.Orders {
    public interface ICartService
    {
        List<CartItemDto> GetCart();
        void SaveCart(List<CartItemDto> cart);
        Task<CartItemDto?> GetProductForCartAsync(int productId);
        int GetCartCount();
        decimal GetCartTotal();
        void ClearCart();
        string? GetCouponCode();
        void SetCouponCode(string? code);
    }
}