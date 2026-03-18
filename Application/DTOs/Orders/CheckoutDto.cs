using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Orders {
    public class CheckoutDto
    {
        [Required(ErrorMessage = "Vui lòng nh?p h? tên")]
        [MaxLength(100, ErrorMessage = "H? tên không quá 100 ký t?")]
        [Display(Name = "H? và tên")]
        public string FullName { get; set; } = "";

        [Required]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; } = "";


        [Required(ErrorMessage = "Vui lòng nh?p email")]
        [EmailAddress(ErrorMessage = "Email không h?p l?")]
        public string Email { get; set; } = "";

        [Required]
        [MaxLength(500)]
        public string ShippingAddress { get; set; } = "";
        public string? Note { get; set; }  
        public string PaymentMethod { get; set; } = "COD";

    }
}
