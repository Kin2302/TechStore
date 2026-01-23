using Domain.Common;

namespace TechStore.Domain.Entities
{
    public class Review : BaseEntity
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }

        public int Rating { get; set; } // 1-5
        public string? Comment { get; set; }
    }
}