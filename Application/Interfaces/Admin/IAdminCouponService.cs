using Application.DTOs.Catalog;

namespace Application.Interfaces.Admin
{
    public interface IAdminCouponService
    {
        Task<List<CouponDto>> GetAllAsync();
        Task<CouponDto?> GetByIdAsync(int id);
        Task CreateAsync(CouponDto dto);
        Task UpdateAsync(CouponDto dto);
        Task DeleteAsync(int id);
        Task<CouponDto?> GetByCodeAsync(string code);
    }
}
