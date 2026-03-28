using Application.DTOs.Orders;
using Application.Interfaces.Orders;
using Microsoft.EntityFrameworkCore;
using TechStore.Domain.Enums;
using TechStore.Infrastructure.Data;

namespace TechStore.Infrastructure.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly ApplicationDbContext _context;

        public VoucherService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<VoucherValidationResultDto> ValidateVoucherAsync(
            string? code,
            string userId,
            decimal subTotal,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return new VoucherValidationResultDto
                {
                    IsValid = false,
                    Message = "Vui lòng nhập mã voucher."
                };
            }

            var normalizedCode = code.Trim().ToUpperInvariant();
            var now = DateTime.UtcNow;

            var voucher = await _context.Vouchers
                .Include(x => x.Usages)
                .FirstOrDefaultAsync(x =>
                    !x.IsDeleted &&
                    x.IsActive &&
                    x.Code == normalizedCode,
                    cancellationToken);

            if (voucher == null)
            {
                return new VoucherValidationResultDto
                {
                    IsValid = false,
                    Message = "Mã voucher không tồn tại hoặc đã bị vô hiệu hóa."
                };
            }

            if (now < voucher.StartAt || now > voucher.EndAt)
            {
                return new VoucherValidationResultDto
                {
                    IsValid = false,
                    Message = "Voucher chưa đến hạn hoặc đã hết hạn."
                };
            }

            if (subTotal < voucher.MinOrderAmount)
            {
                return new VoucherValidationResultDto
                {
                    IsValid = false,
                    Message = $"Đơn hàng tối thiểu {voucher.MinOrderAmount:#,##0}₫ để dùng voucher."
                };
            }

            if (voucher.UsageLimit.HasValue && voucher.UsedCount >= voucher.UsageLimit.Value)
            {
                return new VoucherValidationResultDto
                {
                    IsValid = false,
                    Message = "Voucher đã hết lượt sử dụng."
                };
            }

            if (voucher.MaxUsagePerUser.HasValue)
            {
                var usedByUser = voucher.Usages.Count(x => x.UserId == userId);
                if (usedByUser >= voucher.MaxUsagePerUser.Value)
                {
                    return new VoucherValidationResultDto
                    {
                        IsValid = false,
                        Message = "Bạn đã dùng hết số lượt cho voucher này."
                    };
                }
            }

            var discount = voucher.Type == VoucherType.Percentage
                ? subTotal * (voucher.Value / 100m)
                : voucher.Value;

            if (voucher.MaxDiscountAmount.HasValue && discount > voucher.MaxDiscountAmount.Value)
            {
                discount = voucher.MaxDiscountAmount.Value;
            }

            if (discount > subTotal)
            {
                discount = subTotal;
            }

            return new VoucherValidationResultDto
            {
                IsValid = true,
                Message = "Áp dụng voucher thành công.",
                Code = voucher.Code,
                DiscountAmount = Math.Round(discount, 0)
            };
        }

        public async Task<bool> MarkVoucherAsUsedAsync(
            string code,
            string userId,
            int orderId,
            decimal discountAmount,
            CancellationToken cancellationToken = default)
        {
            var normalizedCode = code.Trim().ToUpperInvariant();

            var voucher = await _context.Vouchers
                .Include(x => x.Usages)
                .FirstOrDefaultAsync(x =>
                    !x.IsDeleted &&
                    x.IsActive &&
                    x.Code == normalizedCode,
                    cancellationToken);

            if (voucher == null) return false;

            if (voucher.UsageLimit.HasValue && voucher.UsedCount >= voucher.UsageLimit.Value)
            {
                return false;
            }

            if (voucher.MaxUsagePerUser.HasValue)
            {
                var usedByUser = voucher.Usages.Count(x => x.UserId == userId);
                if (usedByUser >= voucher.MaxUsagePerUser.Value)
                {
                    return false;
                }
            }

            voucher.UsedCount += 1;

            _context.VoucherUsages.Add(new TechStore.Domain.Entities.VoucherUsage
            {
                VoucherId = voucher.Id,
                UserId = userId,
                OrderId = orderId,
                DiscountAmount = discountAmount,
                UsedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}