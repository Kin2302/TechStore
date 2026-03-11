using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        public string Name { get; set; } = default!;

        public string? Slug { get; set; }
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public int? ParentId { get; set; }
    }
}