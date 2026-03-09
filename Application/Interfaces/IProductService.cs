using Application.DTOs;
using TechStore.Domain.Entities;

namespace Application.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllAsync();
        Task<List<ProductDto>> GetFeaturedAsync(int count = 12);
        Task<ProductDetailDto?> GetByIdAsync(int id);
        Task<List<ProductInfoDto>> FilterByAnalysisAsync(AnalysisResultDto analysis, int maxCount = 30);
        Task<List<ProductInfoDto>> GetPopularAsync(int count);
        Task<List<ProductDto>> SearchProductsAsync(string keyword, decimal? maxPrice = null, int limit = 10);

        // Search page
        Task<List<ProductDto>> SearchAsync(string? keyword, int? categoryId, decimal? minPrice, decimal? maxPrice, string? sortBy);
        Task<List<Category>> GetCategoriesAsync();
    }
}