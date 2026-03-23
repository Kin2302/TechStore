using Application.DTOs.Catalog;
using TechStore.Domain.Entities;

namespace Application.Interfaces.Admin
{
    public interface IAdminProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync(string? search, int? categoryId);
        Task<ProductEditDto?> GetByIdForEditAsync(int id);
        Task CreateAsync(ProductCreateDto dto, string? imageUrl);
        Task UpdateAsync(ProductEditDto dto, string? newImageUrl);
        Task DeleteAsync(int id);
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<IEnumerable<Brand>> GetBrandsAsync();
        Task<List<ProductDto>> GetLowStockProductsAsync(int threshold);
        Task<bool> UpdateProductStockAsync(int productId, int newQuantity);
    }
}
