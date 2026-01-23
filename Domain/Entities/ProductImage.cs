using Domain.Common;

namespace TechStore.Domain.Entities
{
    public class ProductImage : BaseEntity
    {
        public string ImageUrl { get; set; }
        public bool IsMain { get; set; } = false; // Có phải ảnh đại diện không?
        public int SortOrder { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}