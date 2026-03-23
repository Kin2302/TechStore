using Application.DTOs.Catalog;
using Application.Interfaces.Catalog;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace TechStore.Infrastructure.Services
{
    public class WishlistService : IWishlistService
    {
        private const string WishlistSessionKey = "WishlistProductIds";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductService _productService;

        public WishlistService(IHttpContextAccessor httpContextAccessor, IProductService productService)
        {
            _httpContextAccessor = httpContextAccessor;
            _productService = productService;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public List<int> GetWishlistProductIds()
        {
            var json = Session.GetString(WishlistSessionKey);
            return string.IsNullOrWhiteSpace(json)
                ? new List<int>()
                : JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }

        public async Task<List<CompareItemDto>> GetWishlistProductsAsync()
        {
            var ids = GetWishlistProductIds();
            if (!ids.Any()) return new List<CompareItemDto>();

            var products = await _productService.GetCompareProductsAsync(ids);
            // If some ids no longer exist (product removed/hidden), update session to keep counts consistent
            var foundIds = products.Select(p => p.ProductId).ToList();
            if (foundIds.Count != ids.Count)
            {
                // preserve original ordering for found ids
                var orderedFound = ids.Where(id => foundIds.Contains(id)).ToList();
                Save(orderedFound);
            }

            return products.OrderBy(p => ids.IndexOf(p.ProductId)).ToList();
        }

        public async Task<(bool Success, string Message, int Count)> AddProductAsync(int productId)
        {
            var ids = GetWishlistProductIds();
            if (ids.Contains(productId))
            {
                return (false, "S?n ph?m ?ã có trong danh sách yêu thích.", ids.Count);
            }

            var check = await _productService.GetCompareProductsAsync(new List<int> { productId });
            if (!check.Any())
            {
                return (false, "S?n ph?m không t?n t?i ho?c ?ã b? ?n.", ids.Count);
            }

            ids.Add(productId);
            Save(ids);
            return (true, "?ã thêm vào danh sách yêu thích.", ids.Count);
        }

        public int RemoveProduct(int productId)
        {
            var ids = GetWishlistProductIds();
            ids.Remove(productId);
            Save(ids);
            return ids.Count;
        }

        public void Clear()
        {
            Session.Remove(WishlistSessionKey);
        }

        public int GetCount() => GetWishlistProductIds().Count;

        private void Save(List<int> ids)
        {
            Session.SetString(WishlistSessionKey, JsonSerializer.Serialize(ids));
        }
    }
}
