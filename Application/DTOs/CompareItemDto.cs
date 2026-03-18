namespace Application.DTOs
{
    public class CompareItemDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public string? CategoryName { get; set; }
        public string? BrandName { get; set; }
        public string? ShortDescription { get; set; }
        public List<ProductSpecDto> Specifications { get; set; } = new();
    }
}