namespace Application.DTOs.Catalog {
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public string? CategoryName { get; set; }
        public string? BrandName { get; set; }
        public string? ShortDescription { get; set; }
    }

    public class ProductDetailDto : ProductDto
    {
        public string? Description { get; set; }
        public List<ProductImageDto> Images { get; set; } = new();
        public List<ProductSpecDto> Specifications { get; set; } = new();

        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<ReviewDto> Reviews { get; set; } = new();
    }

    public class ProductImageDto
    {
        public string ImageUrl { get; set; } = "";
        public bool IsMain { get; set; }
    }

    public class ProductSpecDto
    {
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class ProductInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string CategoryName { get; set; } = "";
    }
}