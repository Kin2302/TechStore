using System.Security.Cryptography;
using System.Text;
using Application.DTOs.Integration;
using Application.Interfaces.Integration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace TechStore.Infrastructure.Services
{
    public class VNPayService : IVNPayService
    {
        private readonly VNPayOptions _options;

        public VNPayService(IOptions<VNPayOptions> options)
        {
            _options = options.Value;
        }

        public string CreatePaymentUrl(
            HttpContext httpContext,
            string txnRef,
            long amount,
            string orderInfo,
            string? returnUrl = null)
        {
            var finalReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? _options.ReturnUrl : returnUrl;

            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var createDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
            var expireDate = createDate.AddMinutes(15);

            var ipAddress = httpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";

            var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["vnp_Version"] = _options.Version,
                ["vnp_Command"] = _options.Command,
                ["vnp_TmnCode"] = _options.TmnCode,
                ["vnp_Amount"] = (amount * 100).ToString(),
                ["vnp_CurrCode"] = _options.CurrCode,
                ["vnp_TxnRef"] = txnRef,
                ["vnp_OrderInfo"] = orderInfo,
                ["vnp_OrderType"] = _options.OrderType,
                ["vnp_Locale"] = _options.Locale,
                ["vnp_ReturnUrl"] = finalReturnUrl,
                ["vnp_IpAddr"] = ipAddress,
                ["vnp_CreateDate"] = createDate.ToString("yyyyMMddHHmmss"),
                ["vnp_ExpireDate"] = expireDate.ToString("yyyyMMddHHmmss")
            };

            var queryString = BuildRequestQuery(vnpParams);
            var secureHash = ComputeHmacSha512(_options.HashSecret, queryString);

            return $"{_options.BaseUrl}?{queryString}&vnp_SecureHash={secureHash}";
        }

        public bool ValidateReturnSignature(string rawQueryString)
        {
            if (string.IsNullOrWhiteSpace(rawQueryString))
            {
                return false;
            }

            var query = rawQueryString.TrimStart('?');
            var pairs = query.Split('&', StringSplitOptions.RemoveEmptyEntries);

            var sorted = new SortedDictionary<string, string>(StringComparer.Ordinal);
            string? receivedHash = null;

            foreach (var pair in pairs)
            {
                var idx = pair.IndexOf('=');
                var key = idx >= 0 ? pair[..idx] : pair;
                var value = idx >= 0 ? pair[(idx + 1)..] : string.Empty; // GIỮ NGUYÊN ENCODED VALUE

                if (key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase))
                {
                    receivedHash = value;
                    continue;
                }

                if (key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
                {
                    sorted[key] = value;
                }
            }

            if (string.IsNullOrWhiteSpace(receivedHash))
            {
                return false;
            }

            var hashData = string.Join("&", sorted.Select(x => $"{x.Key}={x.Value}"));
            var calculatedHash = ComputeHmacSha512(_options.HashSecret, hashData);

            return string.Equals(receivedHash, calculatedHash, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildRequestQuery(SortedDictionary<string, string> data)
        {
            return string.Join("&", data
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(x => $"{x.Key}={VnPayEncode(x.Value)}"));
        }

        private static string VnPayEncode(string value)
            => Uri.EscapeDataString(value).Replace("%20", "+");

        private static string ComputeHmacSha512(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using var hmac = new HMACSHA512(keyBytes);
            var hash = hmac.ComputeHash(dataBytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}