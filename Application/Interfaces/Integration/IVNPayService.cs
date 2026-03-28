using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Integration
{
    public interface IVNPayService
    {
        string CreatePaymentUrl(
            HttpContext httpContext,
            string txnRef,
            long amount,
            string orderInfo,
            string? returnUrl = null);

        bool ValidateReturnSignature(string rawQueryString);
    }
}