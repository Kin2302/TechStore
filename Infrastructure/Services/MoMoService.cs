using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text.Json;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TechStore.Infrastructure.Services
{
    public class MoMoService : IMoMoService
    {
        private readonly HttpClient _httpClient;
        private readonly MoMoOptions _options;
        private readonly ILogger<MoMoService> _logger;

        public MoMoService(
            HttpClient httpClient,
            IOptions<MoMoOptions> options,
            ILogger<MoMoService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<MoMoCreatePaymentResultDto> CreatePaymentAsync(
            string orderId,
            long amount,
            string orderInfo,
            string? returnUrl = null,
            string? notifyUrl = null)
        {
            var reqId = Guid.NewGuid().ToString("N");
            var extraData = string.Empty;
            var finalReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? _options.ReturnUrl : returnUrl;
            var finalNotifyUrl = string.IsNullOrWhiteSpace(notifyUrl) ? _options.NotifyUrl : notifyUrl;

            var rawSignature =
                $"accessKey={_options.AccessKey}" +
                $"&amount={amount}" +
                $"&extraData={extraData}" +
                $"&ipnUrl={finalNotifyUrl}" +
                $"&orderId={orderId}" +
                $"&orderInfo={orderInfo}" +
                $"&partnerCode={_options.PartnerCode}" +
                $"&redirectUrl={finalReturnUrl}" +
                $"&requestId={reqId}" +
                $"&requestType={_options.RequestType}";

            var signature = ComputeHmacSha256(rawSignature, _options.SecretKey);

            var requestBody = new
            {
                partnerCode = _options.PartnerCode,
                partnerName = "TechStore",
                storeId = "TechStore",
                requestId = reqId,
                amount = amount.ToString(),
                orderId,
                orderInfo,
                redirectUrl = finalReturnUrl,
                ipnUrl = finalNotifyUrl,
                lang = _options.Lang,
                requestType = _options.RequestType,
                extraData,
                signature
            };

            var json = JsonSerializer.Serialize(requestBody);
            var response = await _httpClient.PostAsync(
                _options.Endpoint,
                new StringContent(json, Encoding.UTF8, "application/json"));

            var responseText = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("MoMo response: {Response}", responseText);

            if (!response.IsSuccessStatusCode)
            {
                return new MoMoCreatePaymentResultDto
                {
                    ResultCode = -1,
                    Message = "Không gọi được cổng MoMo",
                    RawResponse = responseText
                };
            }

            var data = JsonSerializer.Deserialize<MoMoCreatePaymentResultDto>(
                responseText,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return data ?? new MoMoCreatePaymentResultDto
            {
                ResultCode = -1,
                Message = "Phản hồi MoMo không hợp lệ",
                RawResponse = responseText
            };
        }

        public bool ValidateReturnSignature(IQueryCollection query)
        {
            var signature = query["signature"].ToString();
            if (string.IsNullOrWhiteSpace(signature))
            {
                return false;
            }

            var raw =
                $"accessKey={_options.AccessKey}" +
                $"&amount={query["amount"]}" +
                $"&extraData={query["extraData"]}" +
                $"&message={query["message"]}" +
                $"&orderId={query["orderId"]}" +
                $"&orderInfo={query["orderInfo"]}" +
                $"&orderType={query["orderType"]}" +
                $"&partnerCode={query["partnerCode"]}" +
                $"&payType={query["payType"]}" +
                $"&requestId={query["requestId"]}" +
                $"&responseTime={query["responseTime"]}" +
                $"&resultCode={query["resultCode"]}" +
                $"&transId={query["transId"]}";

            var calculated = ComputeHmacSha256(raw, _options.SecretKey);
            return string.Equals(signature, calculated, StringComparison.OrdinalIgnoreCase);
        }

        private static string ComputeHmacSha256(string rawData, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var dataBytes = Encoding.UTF8.GetBytes(rawData);

            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(dataBytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
