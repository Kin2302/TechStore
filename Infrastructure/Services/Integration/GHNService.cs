using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Application.DTOs.Integration;
using Application.Interfaces.Integration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TechStore.Infrastructure.Services
{
    public class GHNService : IGHNService
    {
        private readonly HttpClient _httpClient;
        private readonly GHNOptions _options;
        private readonly ILogger<GHNService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public GHNService(
            HttpClient httpClient,
            IOptions<GHNOptions> options,
            ILogger<GHNService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;

            if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
            {
                _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
            }

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> PingAsync(CancellationToken cancellationToken = default)
        {
            var provinces = await GetProvincesAsync(cancellationToken);
            return provinces.Count > 0;
        }

        public async Task<IReadOnlyList<GHNProvinceDto>> GetProvincesAsync(CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "master-data/province");
            AddAuthHeaders(request);

            return await SendAndReadDataAsync<GHNProvinceDto>(request, cancellationToken);
        }

        public async Task<IReadOnlyList<GHNDistrictDto>> GetDistrictsAsync(int provinceId, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "master-data/district");
            AddAuthHeaders(request);

            var payload = JsonSerializer.Serialize(new { province_id = provinceId });
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            return await SendAndReadDataAsync<GHNDistrictDto>(request, cancellationToken);
        }

        public async Task<IReadOnlyList<GHNWardDto>> GetWardsAsync(int districtId, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "master-data/ward");
            AddAuthHeaders(request);

            var payload = JsonSerializer.Serialize(new { district_id = districtId });
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            return await SendAndReadDataAsync<GHNWardDto>(request, cancellationToken);
        }

        public async Task<int?> GetShippingFeeAsync(
            int toDistrictId,
            string toWardCode,
            int insuranceValue,
            CancellationToken cancellationToken = default)
        {
            if (toDistrictId <= 0 || string.IsNullOrWhiteSpace(toWardCode))
            {
                return null;
            }

            if (_options.ShopDistrictId <= 0)
            {
                _logger.LogWarning("GHN ShopDistrictId is missing.");
                return null;
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "v2/shipping-order/fee");
            AddAuthHeaders(request);

            var payload = new
            {
                from_district_id = _options.ShopDistrictId,
                service_type_id = _options.DefaultServiceTypeId,
                to_district_id = toDistrictId,
                to_ward_code = toWardCode,
                height = 10,
                length = 20,
                weight = 1000,
                width = 20,
                insurance_value = insuranceValue
            };

            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var wrapper = await SendAndReadObjectAsync<GHNFeeDataDto>(request, cancellationToken);
            return wrapper?.Data?.Total;
        }

        public async Task<GHNCreateShipmentResultDto?> CreateShipmentAsync(
            GHNCreateShipmentRequestDto req,
            CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "v2/shipping-order/create");
            AddAuthHeaders(request);

            var payload = new
            {
                payment_type_id = 1,
                required_note = "CHOTHUHANG",
                client_order_code = req.ClientOrderCode,
                to_name = req.ToName,
                to_phone = req.ToPhone,
                to_address = req.ToAddress,
                to_ward_code = req.ToWardCode,
                to_district_id = req.ToDistrictId,
                cod_amount = req.CodAmount,
                insurance_value = req.InsuranceValue,
                service_type_id = _options.DefaultServiceTypeId,
                note = req.Note ?? "",
                weight = 1000,
                length = 20,
                width = 20,
                height = 10,
                items = req.Items.Select(x => new
                {
                    name = x.Name,
                    quantity = x.Quantity,
                    price = x.Price,
                    length = x.Length,
                    width = x.Width,
                    height = x.Height,
                    weight = x.Weight
                }).ToList()
            };

            request.Content = new StringContent(
                JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var wrapper = await SendAndReadObjectAsync<GHNCreateShipmentResultDto>(request, cancellationToken);
            return wrapper?.Data;
        }

        public async Task<GHNShipmentDetailDto?> GetShipmentDetailAsync(
            string shippingCode,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(shippingCode)) return null;

            var request = new HttpRequestMessage(HttpMethod.Post, "v2/shipping-order/detail");
            AddAuthHeaders(request);

            request.Content = new StringContent(
                JsonSerializer.Serialize(new { order_code = shippingCode }),
                Encoding.UTF8,
                "application/json");

            var wrapper = await SendAndReadObjectAsync<GHNShipmentDetailDto>(request, cancellationToken);
            return wrapper?.Data;
        }

        private void AddAuthHeaders(HttpRequestMessage request)
        {
            if (!string.IsNullOrWhiteSpace(_options.Token))
            {
                request.Headers.Add("Token", _options.Token);
            }

            if (_options.ShopId > 0)
            {
                request.Headers.Add("ShopId", _options.ShopId.ToString());
            }
        }

        private async Task<IReadOnlyList<T>> SendAndReadDataAsync<T>(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            try
            {
                using var response = await _httpClient.SendAsync(request, cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("GHN API failed. Status: {StatusCode}, Body: {Body}", response.StatusCode, content);
                    return Array.Empty<T>();
                }

                var wrapper = JsonSerializer.Deserialize<GHNDataResponse<T>>(content, JsonOptions);
                if (wrapper == null || wrapper.Code != 200)
                {
                    _logger.LogWarning("GHN API business failed. Code: {Code}, Message: {Message}, Body: {Body}",
                        wrapper?.Code, wrapper?.Message, content);
                    return Array.Empty<T>();
                }

                return wrapper.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GHN API exception.");
                return Array.Empty<T>();
            }
        }

        private async Task<GHNObjectResponse<T>?> SendAndReadObjectAsync<T>(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            try
            {
                using var response = await _httpClient.SendAsync(request, cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("GHN API failed. Status: {StatusCode}, Body: {Body}", response.StatusCode, content);
                    return null;
                }

                var wrapper = JsonSerializer.Deserialize<GHNObjectResponse<T>>(content, JsonOptions);
                if (wrapper == null || wrapper.Code != 200)
                {
                    _logger.LogWarning("GHN API business failed. Code: {Code}, Message: {Message}, Body: {Body}",
                        wrapper?.Code, wrapper?.Message, content);
                    return null;
                }

                return wrapper;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GHN API exception.");
                return null;
            }
        }

        private class GHNDataResponse<T>
        {
            public int Code { get; set; }
            public string Message { get; set; } = string.Empty;
            public List<T> Data { get; set; } = new();
        }

        private class GHNObjectResponse<T>
        {
            public int Code { get; set; }
            public string Message { get; set; } = string.Empty;
            public T? Data { get; set; }
        }
    }
}