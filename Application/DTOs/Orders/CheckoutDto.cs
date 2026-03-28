using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Orders
{
    public class CheckoutDto
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [MaxLength(100, ErrorMessage = "Họ tên không quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = "";

        [Required]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        [Required]
        [MaxLength(500)]
        public string ShippingAddress { get; set; } = "";

        public string? Note { get; set; }
        public string PaymentMethod { get; set; } = "COD";

        public int ProvinceId { get; set; }
        public int DistrictId { get; set; }
        public string WardCode { get; set; } = "";
        public decimal ShippingFee { get; set; }

        public string? VoucherCode { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
