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
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TechStore.Infrastructure.Data;

namespace TechStore.Infrastructure.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CartSessionKey = "Cart";

        public CartService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public List<CartItemDto> GetCart()
        {
            var json = Session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(json) 
                ? new List<CartItemDto>() 
                : JsonSerializer.Deserialize<List<CartItemDto>>(json) ?? new List<CartItemDto>();
        }

        public void SaveCart(List<CartItemDto> cart)
        {
            var json = JsonSerializer.Serialize(cart);
            Session.SetString(CartSessionKey, json);
        }

        public async Task<CartItemDto?> GetProductForCartAsync(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted);

            if (product == null) return null;

            return new CartItemDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ImageUrl = product.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl
                           ?? product.Images.FirstOrDefault()?.ImageUrl,
                Price = product.Price
            };
        }

        public int GetCartCount()
        {
            return GetCart().Sum(x => x.Quantity);
        }

        public decimal GetCartTotal()
        {
            return GetCart().Sum(x => x.Total);
        }

        public void ClearCart()
        {
            Session.Remove(CartSessionKey);
        }
    }
}
