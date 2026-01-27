using Application.DTOs;

namespace Application.Interfaces
{
    public interface ICartService
    {
        List<CartItemDto> GetCart();
        void AddToCart(int productId, int quantity = 1);
        void UpdateQuantity(int productId, int quantity);
        void RemoveFromCart(int productId);
        void ClearCart();
        int GetCartCount();
        decimal GetCartTotal();
    }
}