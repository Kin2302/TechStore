using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Catalog {
    public class BrandDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhậpp tên thuong hiệu")]
        public string Name { get; set; } = default!;

        public string? Origin { get; set; }
        public string? LogoUrl { get; set; }
        public int ProductCount { get; set; }
    }
}