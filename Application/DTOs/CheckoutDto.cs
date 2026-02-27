using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
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

    }
}
