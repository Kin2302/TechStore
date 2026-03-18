namespace Application.DTOs
{
    public class MoMoOptions
    {
        public string PartnerCode { get; set; } = "";
        public string AccessKey { get; set; } = "";
        public string SecretKey { get; set; } = "";
        public string Endpoint { get; set; } = "https://test-payment.momo.vn/v2/gateway/api/create";
        public string ReturnUrl { get; set; } = "";
        public string NotifyUrl { get; set; } = "";
        public string RequestType { get; set; } = "captureWallet";
        public string Lang { get; set; } = "vi";
    }
}