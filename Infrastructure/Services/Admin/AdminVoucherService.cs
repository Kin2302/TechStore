using Application.DTOs.Admin;
using Application.Interfaces.Admin;
using Microsoft.EntityFrameworkCore;
using TechStore.Domain.Entities;
using TechStore.Infrastructure.Data;

namespace Infrastructure.Services.Admin
{
    public class AdminVoucherService : IAdminVoucherService
    {
        private readonly ApplicationDbContext _context;

        public AdminVoucherService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AdminVoucherDto>> GetAllAsync()
        {
            return await _context.Vouchers
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new AdminVoucherDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    Type = x.Type,
                    Value = x.Value,
                    MinOrderAmount = x.MinOrderAmount,
                    MaxDiscountAmount = x.MaxDiscountAmount,
                    StartAt = x.StartAt,
                    EndAt = x.EndAt,
                    IsActive = x.IsActive,
                    UsageLimit = x.UsageLimit,
                    UsedCount = x.UsedCount,
                    MaxUsagePerUser = x.MaxUsagePerUser
                })
                .ToListAsync();
        }

        public async Task<AdminVoucherDto?> GetByIdAsync(int id)
        {
            return await _context.Vouchers
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new AdminVoucherDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    Type = x.Type,
                    Value = x.Value,
                    MinOrderAmount = x.MinOrderAmount,
                    MaxDiscountAmount = x.MaxDiscountAmount,
                    StartAt = x.StartAt,
                    EndAt = x.EndAt,
                    IsActive = x.IsActive,
                    UsageLimit = x.UsageLimit,
                    UsedCount = x.UsedCount,
                    MaxUsagePerUser = x.MaxUsagePerUser
                })
                .FirstOrDefaultAsync();
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(AdminVoucherDto dto)
        {
            var normalizedCode = NormalizeCode(dto.Code);
            var existed = await _context.Vouchers.AnyAsync(x => !x.IsDeleted && x.Code == normalizedCode);
            if (existed)
            {
                return (false, "Mã voucher đã tồn tại.");
            }

            var voucher = new Voucher
            {
                Code = normalizedCode,
                Description = dto.Description?.Trim(),
                Type = dto.Type,
                Value = dto.Value,
                MinOrderAmount = dto.MinOrderAmount,
                MaxDiscountAmount = dto.MaxDiscountAmount,
                StartAt = dto.StartAt,
                EndAt = dto.EndAt,
                IsActive = dto.IsActive,
                UsageLimit = dto.UsageLimit,
                MaxUsagePerUser = dto.MaxUsagePerUser,
                UsedCount = 0
            };

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(AdminVoucherDto dto)
        {
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(x => x.Id == dto.Id && !x.IsDeleted);
            if (voucher == null)
            {
                return (false, "Voucher không tồn tại.");
            }

            var normalizedCode = NormalizeCode(dto.Code);
            var existed = await _context.Vouchers.AnyAsync(x =>
                !x.IsDeleted &&
                x.Id != dto.Id &&
                x.Code == normalizedCode);

            if (existed)
            {
                return (false, "Mã voucher đã tồn tại.");
            }

            voucher.Code = normalizedCode;
            voucher.Description = dto.Description?.Trim();
            voucher.Type = dto.Type;
            voucher.Value = dto.Value;
            voucher.MinOrderAmount = dto.MinOrderAmount;
            voucher.MaxDiscountAmount = dto.MaxDiscountAmount;
            voucher.StartAt = dto.StartAt;
            voucher.EndAt = dto.EndAt;
            voucher.IsActive = dto.IsActive;
            voucher.UsageLimit = dto.UsageLimit;
            voucher.MaxUsagePerUser = dto.MaxUsagePerUser;
            voucher.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<bool> SetActiveAsync(int id, bool isActive)
        {
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (voucher == null)
            {
                return false;
            }

            voucher.IsActive = isActive;
            voucher.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        private static string NormalizeCode(string code)
        {
            return code.Trim().ToUpperInvariant();
        }
    }
}