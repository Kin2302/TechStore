using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;
using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace TechStore.Infrastructure.Plugins
{
    /// <summary>
    /// Plugin cho AI Agent de tim kiem va lay thong tin san pham
    /// </summary>
    public class ProductPlugin
    {
        private readonly IProductService _productService;

        public ProductPlugin(IProductService productService)
        {
            _productService = productService;
        }

        [KernelFunction("search_products")]
        [Description("Tim kiem san pham theo tu khoa, gia, danh muc. Tra ve danh sach san pham phu hop.")]
        public async Task<string> SearchProductsAsync(
            [Description("Tu khoa tim kiem (vi du: 'Arduino', 'ESP32', 'cam bien nhiet do')")]
            string keyword,

            [Description("Gia toi da (VND). De trong neu khong gioi han")]
            decimal? maxPrice = null,

            [Description("So luong ket qua toi da")]
            int limit = 5)
        {
            var products = await _productService.SearchProductsAsync(keyword, maxPrice, limit);

            if (!products.Any())
                return System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    message = $"Khong tim thay san pham nao voi tu khoa '{keyword}'",
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
                message = $"Tim thay {result.Count} san pham",
                products = result
            });
        }

        [KernelFunction("get_product_details")]
        [Description("Lay thong tin chi tiet cua mot san pham cu the theo ID")]
        public async Task<string> GetProductDetailsAsync(
            [Description("ID cua san pham can xem chi tiet")]
            int productId)
        {
            var product = await _productService.GetByIdAsync(productId);

            if (product == null)
                return $"Khong tim thay san pham voi ID {productId}";

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
        [Description("Lay danh sach san pham ban chay nhat")]
        public async Task<string> GetPopularProductsAsync(
            [Description("So luong san pham can lay")]
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

        [KernelFunction("check_product_stock")]
        [Description("Kiem tra tinh trang ton kho cua san pham theo ID. Tra ve so luong con lai va trang thai con hang hay het.")]
        public async Task<string> CheckProductStockAsync(
            [Description("ID cua san pham can kiem tra ton kho")]
            int productId)
        {
            var product = await _productService.GetByIdAsync(productId);

            if (product == null)
                return System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    message = $"Khong tim thay san pham voi ID {productId}"
                });

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                success = true,
                productId = product.Id,
                productName = product.Name,
                stock = product.Stock,
                inStock = product.Stock > 0,
                message = product.Stock > 0
                    ? $"{product.Name} con {product.Stock} san pham trong kho"
                    : $"{product.Name} hien da het hang"
            });
        }

        [KernelFunction("suggest_project_components")]
        [Description("Goi y cac linh kien can thiet cho mot du an/project dien tu (VD: xe tu hanh, nha thong minh, robot). Tim kiem san pham phu hop trong cua hang va de xuat danh sach linh kien. Goi ham nay khi khach hang hoi ve project hoac muon biet can mua gi cho du an.")]
        public async Task<string> SuggestProjectComponentsAsync(
            [Description("Mo ta du an cua khach hang (VD: 'xe tu hanh', 'nha thong minh', 'robot do duong')")]
            string projectDescription,

            [Description("Ngan sach toi da (VND), de trong neu khong gioi han")]
            decimal? maxBudget = null)
        {
            try
            {
                var keywords = projectDescription.ToLower().Split(' ', ',', '-', '.', '!', '?');
                var allProducts = new List<ProductDto>();

                foreach (var keyword in keywords.Where(k => k.Length > 2))
                {
                    var results = await _productService.SearchProductsAsync(keyword, maxBudget, 10);
                    allProducts.AddRange(results);
                }

                var mainResults = await _productService.SearchProductsAsync(projectDescription, maxBudget, 15);
                allProducts.AddRange(mainResults);

                var uniqueProducts = allProducts
                    .GroupBy(p => p.Id)
                    .Select(g => g.First())
                    .Where(p => p.Stock > 0)
                    .Take(20)
                    .ToList();

                if (!uniqueProducts.Any())
                {
                    return System.Text.Json.JsonSerializer.Serialize(new
                    {
                        success = false,
                        message = $"Khong tim thay linh kien phu hop cho du an '{projectDescription}' trong cua hang.",
                        suggestion = "Ban co the mo ta chi tiet hon hoac thu tu khoa khac."
                    });
                }

                var totalPrice = uniqueProducts.Sum(p => p.Price);
                var components = uniqueProducts.Select(p => new
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
                    projectDescription = projectDescription,
                    message = $"Tim thay {components.Count} linh kien co the phu hop cho du an '{projectDescription}'",
                    totalEstimatedPrice = totalPrice,
                    maxBudget = maxBudget,
                    withinBudget = maxBudget == null || totalPrice <= maxBudget,
                    components = components,
                    note = "Hay chon linh kien phu hop nhat. Neu muon them vao gio hang, hay noi 'them san pham ID [so] vao gio' hoac 'them tat ca vao gio'."
                });
            }
            catch (Exception ex)
            {
                return System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    message = $"Loi khi tim kiem linh kien: {ex.Message}"
                });
            }
        }
    }
}
