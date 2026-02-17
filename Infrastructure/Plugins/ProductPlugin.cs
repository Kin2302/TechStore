using System.ComponentModel;
using Application.Interfaces;
using Microsoft.SemanticKernel;

namespace TechStore.Infrastructure.Plugins
{
    /// <summary>
    /// Plugin cho AI Agent để tìm kiếm và lấy thông tin sản phẩm
    /// </summary>
    public class ProductPlugin
    {
        private readonly IProductService _productService;

        public ProductPlugin(IProductService productService)
        {
            _productService = productService;
        }

        [KernelFunction("search_products")]
        [Description("Tìm kiếm sản phẩm theo từ khóa, giá, danh mục. Trả về danh sách sản phẩm phù hợp.")]
        public async Task<string> SearchProductsAsync(
            [Description("Từ khóa tìm kiếm (ví dụ: 'Arduino', 'ESP32', 'cảm biến nhiệt độ')")] 
            string keyword,
            
            [Description("Giá tối đa (VND). Để trống nếu không giới hạn")] 
            decimal? maxPrice = null,
            
            [Description("Số lượng kết quả tối đa")] 
            int limit = 5)
        {
            var products = await _productService.SearchProductsAsync(keyword, maxPrice, limit);

            if (!products.Any())
                return System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    message = $"❌ Không tìm thấy sản phẩm nào với từ khóa '{keyword}'",
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
                message = $"✅ Tìm thấy {result.Count} sản phẩm",
                products = result
            });
        }

        [KernelFunction("get_product_details")]
        [Description("Lấy thông tin chi tiết của một sản phẩm cụ thể theo ID")]
        public async Task<string> GetProductDetailsAsync(
            [Description("ID của sản phẩm cần xem chi tiết")] 
            int productId)
        {
            var product = await _productService.GetByIdAsync(productId);
            
            if (product == null)
                return $"❌ Không tìm thấy sản phẩm với ID {productId}";

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
        [Description("Lấy danh sách sản phẩm bán chạy nhất")]
        public async Task<string> GetPopularProductsAsync(
            [Description("Số lượng sản phẩm cần lấy")] 
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