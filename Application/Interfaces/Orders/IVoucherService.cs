using Application.DTOs.Orders;

namespace Application.Interfaces.Orders
{
    public interface IVoucherService
    {
        Task<VoucherValidationResultDto> ValidateVoucherAsync(
            string? code,
            string userId,
            decimal subTotal,
            CancellationToken cancellationToken = default);

        Task<bool> MarkVoucherAsUsedAsync(
            string code,
            string userId,
            int orderId,
            decimal discountAmount,
            CancellationToken cancellationToken = default);
    }
}