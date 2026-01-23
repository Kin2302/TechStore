using Domain.Common;

namespace TechStore.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public string Code { get; set; } // SKU
        public string Slug { get; set; }
        public string? ShortDescription { get; set; }
        public string? Description { get; set; } // HTML

        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int Stock { get; set; }
        public int SoldCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;

        // Khóa ngoại
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int? BrandId { get; set; }
        public Brand? Brand { get; set; }

        // Quan hệ 1-N
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductSpecification> Specifications { get; set; } = new List<ProductSpecification>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}