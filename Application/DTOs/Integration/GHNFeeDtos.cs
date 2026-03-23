using System.Text.Json.Serialization;

namespace Application.DTOs.Integration
{
    public class GHNFeeDataDto
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}