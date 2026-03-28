using Domain.Common;

namespace TechStore.Domain.Entities
{
    public class WishlistItem : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;
    }
}