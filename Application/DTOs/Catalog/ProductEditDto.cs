using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Catalog {
    public class ProductEditDto
    {
        [Required(ErrorMessage = "Vui lòng nh?p tên")]
        public string Name { get; set; } = default!;

        [Required]
        public string Code { get; set; } = default!;

        public string? ShortDescription { get; set; }

        public string? Description { get; set; }

        [Required]
        [Range(0, 999999999, ErrorMessage = "Giá ph?i >= 0")]
        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public int? BrandId { get; set; }

        public bool IsActive { get; set; }

        public bool IsFeatured { get; set; }

        public int Id { get; set; }
        public string? ExistingImageUrl { get; set; }

        public List<SpecificationInputDto> Specifications { get; set; } = new();
    }
}
