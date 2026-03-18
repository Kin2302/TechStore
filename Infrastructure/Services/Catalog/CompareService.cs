using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;
using System.Text.Json;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace TechStore.Infrastructure.Services
{
    public class CompareService : ICompareService
    {
        private const string CompareSessionKey = "CompareProductIds";
        private const int MaxCompareItems = 3;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductService _productService;

        public CompareService(IHttpContextAccessor httpContextAccessor, IProductService productService)
        {
            _httpContextAccessor = httpContextAccessor;
            _productService = productService;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public List<int> GetCompareProductIds()
        {
            var json = Session.GetString(CompareSessionKey);
            return string.IsNullOrWhiteSpace(json)
                ? new List<int>()
                : JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }

        public async Task<List<CompareItemDto>> GetCompareProductsAsync()
        {
            var ids = GetCompareProductIds();
            if (!ids.Any()) return new List<CompareItemDto>();

            var products = await _productService.GetCompareProductsAsync(ids);
            return products.OrderBy(p => ids.IndexOf(p.ProductId)).ToList();
        }

        public async Task<(bool Success, string Message, int Count)> AddProductAsync(int productId)
        {
            var ids = GetCompareProductIds();

            if (ids.Contains(productId))
            {
                return (false, "Sản phẩm đã có trong danh sách so sánh.", ids.Count);
            }

            if (ids.Count >= MaxCompareItems)
            {
                return (false, $"Chỉ so sánh tối đa {MaxCompareItems} sản phẩm.", ids.Count);
            }

            var check = await _productService.GetCompareProductsAsync(new List<int> { productId });
            if (!check.Any())
            {
                return (false, "Sản phẩm không tồn tại hoặc đã bị ẩn.", ids.Count);
            }

            ids.Add(productId);
            Save(ids);

            return (true, "Đã thêm vào danh sách so sánh.", ids.Count);
        }

        public int RemoveProduct(int productId)
        {
            var ids = GetCompareProductIds();
            ids.Remove(productId);
            Save(ids);
            return ids.Count;
        }

        public void Clear()
        {
            Session.Remove(CompareSessionKey);
        }

        public int GetCount() => GetCompareProductIds().Count;

        private void Save(List<int> ids)
        {
            Session.SetString(CompareSessionKey, JsonSerializer.Serialize(ids));
        }
    }
}