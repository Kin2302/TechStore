using Application.DTOs;

namespace Application.Interfaces
{
    public interface IAdminCategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
        Task CreateAsync(CategoryDto dto);
        Task UpdateAsync(CategoryDto dto);
        Task DeleteAsync(int id);
        Task<IEnumerable<CategoryDto>> GetParentCategoriesAsync();
    }
}