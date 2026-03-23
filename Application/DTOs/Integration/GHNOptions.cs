namespace Application.DTOs.Integration
{
    public class GHNOptions
    {
        public string BaseUrl { get; set; } = "https://online-gateway.ghn.vn/shiip/public-api";
        public string Token { get; set; } = "";
        public int ShopId { get; set; }

        // Quận lấy hàng của shop (GHN yêu cầu khi tính phí)
        public int ShopDistrictId { get; set; }

        // GHN thường dùng 2 = Hàng nhẹ
        public int DefaultServiceTypeId { get; set; } = 2;
    }
}