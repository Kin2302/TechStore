using Application.DTOs;

namespace Application.Interfaces
{
    public interface IAdminBrandService
    {
        Task<IEnumerable<BrandDto>> GetAllAsync();
        Task<BrandDto?> GetByIdAsync(int id);
        Task CreateAsync(BrandDto dto, string? logoUrl);
        Task UpdateAsync(BrandDto dto, string? newLogoUrl);
        Task DeleteAsync(int id);
    }
}