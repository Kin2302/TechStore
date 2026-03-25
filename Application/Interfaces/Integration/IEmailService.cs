using Application.DTOs.Catalog;
using Application.DTOs.Orders;

namespace Application.Interfaces.Integration
{
    public interface IEmailService
    {
        Task SendOrderCreatedEmailAsync(
            string toEmail,
            int orderId,
            string customerName,
            decimal totalAmount,
            decimal shippingFee,
            IEnumerable<CartItemDto> items);
    }
}