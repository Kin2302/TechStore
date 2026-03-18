using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Catalog {
    public class ReviewDto
    {
        public string UserName { get; set; } = "";
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CreateReviewDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Sản phẩm không hợp lệ")]
        public int ProductId { get; set; }

        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5")]
        public int Rating { get; set; } = 5;

        [MaxLength(1000, ErrorMessage = "Nội dung đánh giá không quá 1000 ký tự")]
        public string? Comment { get; set; }
    }
}