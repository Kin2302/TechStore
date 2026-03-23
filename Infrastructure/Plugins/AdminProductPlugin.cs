using Application.DTOs.Catalog;
using Application.Interfaces.Admin;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;

namespace TechStore.Infrastructure.Plugins
{
    /// <summary>
    /// Plugin quan ly san pham va kho hang cho Admin
    /// </summary>
    public class AdminProductPlugin
    {
        private readonly IAdminProductService _adminProductService;

        public AdminProductPlugin(IAdminProductService adminProductService)
        {
            _adminProductService = adminProductService;
        }

        [KernelFunction("create_product")]
        [Description("Tao mot san pham moi tu cau lenh chat. San pham se mac dinh IsActive=false (chua hien thi) de Admin kiem tra truoc. VD: 'tao san pham Arduino Uno gia 150000 danh muc 1 ton kho 50'.")]
        public async Task<string> CreateProductAsync(
            [Description("Ten san pham")]
            string name,

            [Description("Gia san pham (VND)")]
            decimal price,

            [Description("ID danh muc san pham")]
            int categoryId,

            [Description("So luong ton kho")]
            int stock,

            [Description("Ma san pham (SKU), de trong se tu tao")]
            string? code = null,

            [Description("Mo ta ngan")]
            string? shortDescription = null)
        {
            try
            {
                var dto = new ProductCreateDto
                {
                    Name = name,
                    Code = code ?? $"SKU-{DateTime.Now:yyyyMMddHHmmss}",
                    Price = price,
                    Stock = stock,
                    CategoryId = categoryId,
                    ShortDescription = shortDescription,
                    IsActive = false,
                    IsFeatured = false,
                    Specifications = new List<SpecificationInputDto>()
                };

                await _adminProductService.CreateAsync(dto, null);

                return JsonSerializer.Serialize(new
                {
                    success = true,
                    message = $"Da tao san pham '{name}' thanh cong (gia: {price:N0} VND, ton kho: {stock}). San pham dang o trang thai CHUA HIEN THI, vui long vao trang Admin de kiem tra va kich hoat.",
                    product = new { name, price, stock, categoryId, code = dto.Code }
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    message = $"Loi khi tao san pham: {ex.Message}"
                });
            }
        }

        [KernelFunction("create_multiple_products")]
        [Description("Tao nhieu san pham cung luc tu danh sach JSON. VD: Admin gui danh sach san pham can nhap.")]
        public async Task<string> CreateMultipleProductsAsync(
            [Description("Danh sach san pham dang JSON. Format: [{\"name\":\"Ten\",\"price\":100000,\"categoryId\":1,\"stock\":10}]")]
            string productsJson)
        {
            try
            {
                var products = JsonSerializer.Deserialize<List<SimpleProductInput>>(productsJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (products == null || !products.Any())
                {
                    return JsonSerializer.Serialize(new
                    {
                        success = false,
                        message = "Danh sach san pham trong hoac khong hop le."
                    });
                }

                int created = 0;
                var errors = new List<string>();

                foreach (var p in products)
                {
                    try
                    {
                        var dto = new ProductCreateDto
                        {
                            Name = p.Name,
                            Code = p.Code ?? $"SKU-{DateTime.Now:yyyyMMddHHmmss}-{created}",
                            Price = p.Price,
                            Stock = p.Stock,
                            CategoryId = p.CategoryId,
                            ShortDescription = p.ShortDescription,
                            IsActive = false,
                            IsFeatured = false,
                            Specifications = new List<SpecificationInputDto>()
                        };

                        await _adminProductService.CreateAsync(dto, null);
                        created++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Loi '{p.Name}': {ex.Message}");
                    }
                }

                return JsonSerializer.Serialize(new
                {
                    success = created > 0,
                    message = $"Da tao {created}/{products.Count} san pham thanh cong." +
                              (errors.Any() ? $" Loi: {string.Join("; ", errors)}" : ""),
                    createdCount = created,
                    totalCount = products.Count,
                    errors = errors
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    message = $"Loi khi xu ly danh sach san pham: {ex.Message}"
                });
            }
        }

        [KernelFunction("get_low_stock_products")]
        [Description("Lay danh sach san pham sap het hang (ton kho thap). VD: 'san pham nao sap het hang', 'liet ke san pham ton kho duoi 10'.")]
        public async Task<string> GetLowStockProductsAsync(
            [Description("Nguong ton kho toi da (mac dinh 10)")]
            int threshold = 10)
        {
            var products = await _adminProductService.GetLowStockProductsAsync(threshold);

            return JsonSerializer.Serialize(new
            {
                success = true,
                count = products.Count,
                threshold = threshold,
                products = products.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    code = p.Code,
                    stock = p.Stock,
                    category = p.CategoryName
                }),
                message = products.Any()
                    ? $"Co {products.Count} san pham co ton kho duoi {threshold}"
                    : $"Khong co san pham nao co ton kho duoi {threshold}"
            });
        }

        [KernelFunction("update_product_stock")]
        [Description("Cap nhat so luong ton kho cua san pham. VD: 'cap nhat ton kho san pham ID 1 thanh 50', 'san pham X vua nhap ve 100 cai'.")]
        public async Task<string> UpdateProductStockAsync(
            [Description("ID san pham can cap nhat")]
            int productId,

            [Description("So luong ton kho moi")]
            int newQuantity)
        {
            var success = await _adminProductService.UpdateProductStockAsync(productId, newQuantity);

            return JsonSerializer.Serialize(new
            {
                success = success,
                message = success
                    ? $"Da cap nhat ton kho san pham #{productId} thanh {newQuantity}"
                    : $"Khong tim thay san pham #{productId}"
            });
        }

        private class SimpleProductInput
        {
            public string Name { get; set; } = "";
            public decimal Price { get; set; }
            public int CategoryId { get; set; }
            public int Stock { get; set; }
            public string? Code { get; set; }
            public string? ShortDescription { get; set; }
        }
    }
}
