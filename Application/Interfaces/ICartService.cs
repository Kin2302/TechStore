    using Application.DTOs;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICartService
    {
        List<CartItemDto> GetCart();
        void SaveCart(List<CartItemDto> cart);
        Task<CartItemDto?> GetProductForCartAsync(int productId);
        int GetCartCount();
        decimal GetCartTotal();
        void ClearCart();
    }
}