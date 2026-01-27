using Application.DTOs;

namespace Application.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllAsync();
        Task<List<ProductDto>> GetFeaturedAsync(int count = 12);
        Task<ProductDetailDto?> GetByIdAsync(int id);
        Task<List<ProductInfoDto>> FilterByAnalysisAsync(AnalysisResultDto analysis, int maxCount = 30);
        Task<List<ProductInfoDto>> GetPopularAsync(int count);
    }
}