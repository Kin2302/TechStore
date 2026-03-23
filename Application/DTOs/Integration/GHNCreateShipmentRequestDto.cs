using System.Text.Json.Serialization;

namespace Application.DTOs.Integration
{
    public class GHNCreateShipmentRequestDto
    {
        public string ClientOrderCode { get; set; } = "";
        public string ToName { get; set; } = "";
        public string ToPhone { get; set; } = "";
        public string ToAddress { get; set; } = "";
        public int ToDistrictId { get; set; }
        public string ToWardCode { get; set; } = "";
        public int InsuranceValue { get; set; }
        public int CodAmount { get; set; }
        public string? Note { get; set; }
        public List<GHNShipmentItemDto> Items { get; set; } = new();
    }

    public class GHNShipmentItemDto
    {
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public int Price { get; set; }
        public int Length { get; set; } = 20;
        public int Width { get; set; } = 20;
        public int Height { get; set; } = 10;
        public int Weight { get; set; } = 200;
    }

    public class GHNCreateShipmentResultDto
    {
        [JsonPropertyName("order_code")]
        public string? OrderCode { get; set; }
    }

    public class GHNShipmentDetailDto
    {
        [JsonPropertyName("order_code")]
        public string? OrderCode { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }
}