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
    /// Plugin cho AI Agent d? tìm ki?m và l?y thông tin s?n ph?m
    /// </summary>
    public class ProductPlugin
    {
        private readonly IProductService _productService;

        public ProductPlugin(IProductService productService)
        {
            _productService = productService;
        }

        [KernelFunction("search_products")]
        [Description("Tìm ki?m s?n ph?m theo t? khóa, giá, danh m?c. Tr? v? danh sách s?n ph?m phù h?p.")]
        public async Task<string> SearchProductsAsync(
            [Description("T? khóa tìm ki?m (ví d?: 'Arduino', 'ESP32', 'c?m bi?n nhi?t d?')")] 
            string keyword,
            
            [Description("Giá t?i da (VND). Ð? tr?ng n?u không gi?i h?n")] 
            decimal? maxPrice = null,
            
            [Description("S? lu?ng k?t qu? t?i da")] 
            int limit = 5)
        {
            var products = await _productService.SearchProductsAsync(keyword, maxPrice, limit);

            if (!products.Any())
                return System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    message = $"? Không tìm th?y s?n ph?m nào v?i t? khóa '{keyword}'",
                    count = 0,
                    products = Array.Empty<object>()
                });

            var result = products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                price = p.Price,
                stock = p.Stock,
                category = p.CategoryName,
                brand = p.BrandName
            }).ToList();

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                success = true,
                count = result.Count,
                message = $"? Tìm th?y {result.Count} s?n ph?m",
                products = result
            });
        }

        [KernelFunction("get_product_details")]
        [Description("L?y thông tin chi ti?t c?a m?t s?n ph?m c? th? theo ID")]
        public async Task<string> GetProductDetailsAsync(
            [Description("ID c?a s?n ph?m c?n xem chi ti?t")] 
            int productId)
        {
            var product = await _productService.GetByIdAsync(productId);
            
            if (product == null)
                return $"? Không tìm th?y s?n ph?m v?i ID {productId}";

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                success = true,
                product = new
                {
                    id = product.Id,
                    name = product.Name,
                    code = product.Code,
                    price = product.Price,
                    stock = product.Stock,
                    description = product.ShortDescription,
                    category = product.CategoryName,
                    brand = product.BrandName,
                    specifications = product.Specifications.Select(s => new 
                    { 
                        name = s.Name, 
                        value = s.Value 
                    })
                }
            });
        }

        [KernelFunction("get_popular_products")]
        [Description("L?y danh sách s?n ph?m bán ch?y nh?t")]
        public async Task<string> GetPopularProductsAsync(
            [Description("S? lu?ng s?n ph?m c?n l?y")] 
            int count = 5)
        {
            var products = await _productService.GetFeaturedAsync(count);
            
            var result = products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                price = p.Price,
                stock = p.Stock
            }).ToList();

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                success = true,
                count = result.Count,
                products = result
            });
        }
    }
}