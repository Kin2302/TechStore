using Domain.Common;

namespace TechStore.Domain.Entities
{
    public class Brand : BaseEntity
    {
        public string Name { get; set; } // VD: Arduino, Raspberry Pi
        public string? LogoUrl { get; set; }
        public string? Origin { get; set; } // Xuất xứ
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}