using Application.DTOs.Admin;

namespace Application.Interfaces.Admin
{
    public interface IAdminVoucherService
    {
        Task<IEnumerable<AdminVoucherDto>> GetAllAsync();
        Task<AdminVoucherDto?> GetByIdAsync(int id);
        Task<(bool Success, string? ErrorMessage)> CreateAsync(AdminVoucherDto dto);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(AdminVoucherDto dto);
        Task<bool> SetActiveAsync(int id, bool isActive);
    }
}