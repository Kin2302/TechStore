using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface IMoMoService
    {
        Task<MoMoCreatePaymentResultDto> CreatePaymentAsync(
            string orderId,
            long amount,
            string orderInfo,
            string? returnUrl = null,
            string? notifyUrl = null);

        bool ValidateReturnSignature(IQueryCollection query);
    }
}