using System.ComponentModel.DataAnnotations;
using TechStore.Domain.Enums;

namespace Application.DTOs.Admin
{
    public class AdminVoucherDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã voucher")]
        [MaxLength(50, ErrorMessage = "Mã voucher tối đa 50 ký tự")]
        [Display(Name = "Mã voucher")]
        public string Code { get; set; } = string.Empty;

        [MaxLength(250, ErrorMessage = "Mô tả tối đa 250 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Loại giảm giá")]
        public VoucherType Type { get; set; } = VoucherType.FixedAmount;

        [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Giá trị giảm phải lớn hơn 0")]
        [Display(Name = "Giá trị giảm")]
        public decimal Value { get; set; }

        [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Đơn tối thiểu không hợp lệ")]
        [Display(Name = "Đơn tối thiểu")]
        public decimal MinOrderAmount { get; set; }

        [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Giảm tối đa phải lớn hơn 0")]
        [Display(Name = "Giảm tối đa")]
        public decimal? MaxDiscountAmount { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu")]
        [Display(Name = "Bắt đầu")]
        public DateTime StartAt { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng chọn thời gian kết thúc")]
        [Display(Name = "Kết thúc")]
        public DateTime EndAt { get; set; } = DateTime.Now.AddDays(7);

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;

        [Range(1, int.MaxValue, ErrorMessage = "Tổng lượt dùng phải lớn hơn 0")]
        [Display(Name = "Giới hạn tổng lượt")]
        public int? UsageLimit { get; set; }

        [Display(Name = "Đã dùng")]
        public int UsedCount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Lượt dùng mỗi user phải lớn hơn 0")]
        [Display(Name = "Giới hạn mỗi user")]
        public int? MaxUsagePerUser { get; set; }
    }
}