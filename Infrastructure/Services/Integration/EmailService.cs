using System.Net;
using System.Text;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Integration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace TechStore.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpOptions _options;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<SmtpOptions> options, ILogger<EmailService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendOrderCreatedEmailAsync(
            string toEmail,
            int orderId,
            string customerName,
            decimal totalAmount,
            decimal shippingFee,
            IEnumerable<CartItemDto> items)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = $"[TechStore] Xác nhận đơn hàng #{orderId}";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = BuildOrderHtml(orderId, customerName, totalAmount, shippingFee, items)
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();

            var secureSocket = _options.UseSsl
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            await smtp.ConnectAsync(_options.Host, _options.Port, secureSocket);
            await smtp.AuthenticateAsync(_options.UserName, _options.Password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Sent order confirmation email to {Email} for order {OrderId}", toEmail, orderId);
        }

        private static string BuildOrderHtml(
            int orderId,
            string customerName,
            decimal totalAmount,
            decimal shippingFee,
            IEnumerable<CartItemDto> items)
        {
            var sb = new StringBuilder();
            foreach (var i in items)
            {
                sb.Append($"<tr><td>{WebUtility.HtmlEncode(i.ProductName)}</td><td>{i.Quantity}</td><td>{i.Price:N0} đ</td><td>{i.Total:N0} đ</td></tr>");
            }

            return $"""
                <h3>Xin chào {WebUtility.HtmlEncode(customerName)},</h3>
                <p>TechStore đã nhận đơn hàng <b>#{orderId}</b>.</p>
                <table border="1" cellpadding="6" cellspacing="0" style="border-collapse:collapse">
                    <thead><tr><th>Sản phẩm</th><th>SL</th><th>Giá</th><th>Thành tiền</th></tr></thead>
                    <tbody>{sb}</tbody>
                </table>
                <p>Phí vận chuyển: <b>{shippingFee:N0} đ</b></p>
                <p>Tổng thanh toán: <b>{totalAmount:N0} đ</b></p>
                """;
        }
    }
}