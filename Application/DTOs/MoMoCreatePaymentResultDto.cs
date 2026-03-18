namespace Application.DTOs
{
    public class MoMoCreatePaymentResultDto
    {
        public bool Success => ResultCode == 0 && !string.IsNullOrWhiteSpace(PayUrl);
        public int ResultCode { get; set; }
        public string Message { get; set; } = "";
        public string? PayUrl { get; set; }
        public string? Deeplink { get; set; }
        public string? QrCodeUrl { get; set; }
        public string? OrderId { get; set; }
        public string? RequestId { get; set; }
        public string? RawResponse { get; set; }
    }
}