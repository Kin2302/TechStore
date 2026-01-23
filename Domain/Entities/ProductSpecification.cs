using Domain.Common;

namespace TechStore.Domain.Entities
{
    public class ProductSpecification : BaseEntity
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string Name { get; set; }  // VD: Điện áp
        public string Value { get; set; } // VD: 5V
    }
}