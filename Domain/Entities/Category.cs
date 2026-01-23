using Domain.Common;

namespace TechStore.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } // VD: Vi điều khiển
        public string Slug { get; set; } // VD: vi-dieu-khien
        public string? Description { get; set; }
        public string? IconUrl { get; set; }

        // Đệ quy: Danh mục cha - con
        public int? ParentId { get; set; }
        public Category? Parent { get; set; }
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}