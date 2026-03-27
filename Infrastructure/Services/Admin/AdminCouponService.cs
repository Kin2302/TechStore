using Application.DTOs.Catalog;
using Application.Interfaces.Admin;
using Microsoft.EntityFrameworkCore;
using TechStore.Domain.Entities;
using TechStore.Infrastructure.Data;

namespace Infrastructure.Services.Admin
{
    public class AdminCouponService : IAdminCouponService
    {
        private readonly ApplicationDbContext _context;

        public AdminCouponService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CouponDto>> GetAllAsync()
        {
            return await _context.Coupons
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new CouponDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    IsPercent = c.IsPercent,
                    Amount = c.Amount,
                    Description = c.Description,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    UsageLimit = c.UsageLimit,
                    UsedCount = c.UsedCount,
                    IsActive = c.IsActive
                }).ToListAsync();
        }

        public async Task<CouponDto?> GetByIdAsync(int id)
        {
            var c = await _context.Coupons.FindAsync(id);
            if (c == null) return null;
            return new CouponDto
            {
                Id = c.Id,
                Code = c.Code,
                IsPercent = c.IsPercent,
                Amount = c.Amount,
                Description = c.Description,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                UsageLimit = c.UsageLimit,
                UsedCount = c.UsedCount,
                IsActive = c.IsActive
            };
        }

        public async Task CreateAsync(CouponDto dto)
        {
            var c = new Coupon
            {
                Code = dto.Code,
                IsPercent = dto.IsPercent,
                Amount = dto.Amount,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                UsageLimit = dto.UsageLimit,
                UsedCount = dto.UsedCount,
                IsActive = dto.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.Coupons.Add(c);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CouponDto dto)
        {
            var c = await _context.Coupons.FindAsync(dto.Id);
            if (c == null) return;
            c.Code = dto.Code;
            c.IsPercent = dto.IsPercent;
            c.Amount = dto.Amount;
            c.Description = dto.Description;
            c.StartDate = dto.StartDate;
            c.EndDate = dto.EndDate;
            c.UsageLimit = dto.UsageLimit;
            c.UsedCount = dto.UsedCount;
            c.IsActive = dto.IsActive;
            c.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var c = await _context.Coupons.FindAsync(id);
            if (c == null) return;
            _context.Coupons.Remove(c);
            await _context.SaveChangesAsync();
        }

        public async Task<CouponDto?> GetByCodeAsync(string code)
        {
            var c = await _context.Coupons
                .FirstOrDefaultAsync(x => x.Code == code && x.IsActive);
            if (c == null) return null;
            return new CouponDto
            {
                Id = c.Id,
                Code = c.Code,
                IsPercent = c.IsPercent,
                Amount = c.Amount,
                Description = c.Description,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                UsageLimit = c.UsageLimit,
                UsedCount = c.UsedCount,
                IsActive = c.IsActive
            };
        }
    }
}
