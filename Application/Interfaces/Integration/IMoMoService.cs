using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;
using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Integration {
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