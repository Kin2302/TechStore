using System.Text.Json.Serialization;

namespace Application.DTOs.Integration
{
    public class GHNProvinceDto
    {
        [JsonPropertyName("ProvinceID")]
        public int ProvinceId { get; set; }

        [JsonPropertyName("ProvinceName")]
        public string ProvinceName { get; set; } = string.Empty;
    }

    public class GHNDistrictDto
    {
        [JsonPropertyName("DistrictID")]
        public int DistrictId { get; set; }

        [JsonPropertyName("ProvinceID")]
        public int ProvinceId { get; set; }

        [JsonPropertyName("DistrictName")]
        public string DistrictName { get; set; } = string.Empty;
    }

    public class GHNWardDto
    {
        [JsonPropertyName("WardCode")]
        public string WardCode { get; set; } = string.Empty;

        [JsonPropertyName("DistrictID")]
        public int DistrictId { get; set; }

        [JsonPropertyName("WardName")]
        public string WardName { get; set; } = string.Empty;
    }
}