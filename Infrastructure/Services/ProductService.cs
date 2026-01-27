using Application.DTOs;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using TechStore.Infrastructure.Data;

namespace TechStore.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductDto>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Where(p => !p.IsDeleted)
                .Select(p => MapToDto(p))
                .ToListAsync();
        }

        public async Task<List<ProductDto>> GetFeaturedAsync(int count = 12)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => !p.IsDeleted && p.Stock > 0)
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.SoldCount)
                .Take(count)
                .Select(p => MapToDto(p))
                .ToListAsync();
        }

        public async Task<ProductDetailDto?> GetByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Specifications)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null) return null;

            return new ProductDetailDto
            {
                Id = product.Id,
                Name = product.Name,
                Code = product.Code,
                Price = product.Price,
                Stock = product.Stock,
                ShortDescription = product.ShortDescription,
                Description = product.Description,
                CategoryName = product.Category?.Name,
                BrandName = product.Brand?.Name,
                ImageUrl = product.Images?.FirstOrDefault(i => i.IsMain)?.ImageUrl
                    ?? product.Images?.FirstOrDefault()?.ImageUrl,
                Images = product.Images?.Select(i => new ProductImageDto
                {
                    ImageUrl = i.ImageUrl,
                    IsMain = i.IsMain
                }).ToList() ?? new(),
                Specifications = product.Specifications?.Select(s => new ProductSpecDto
                {
                    Name = s.Name,
                    Value = s.Value
                }).ToList() ?? new()
            };
        }

        public async Task<List<ProductInfoDto>> FilterByAnalysisAsync(AnalysisResultDto analysis, int maxCount = 30)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted && p.Stock > 0);

            if (analysis.Categories.Any())
            {
                query = query.Where(p => p.Category != null && analysis.Categories.Contains(p.Category.Name));
            }

            if (analysis.Keywords.Any())
            {
                query = query.Where(p => analysis.Keywords.Any(k => p.Name.ToLower().Contains(k.ToLower())));
            }

            return await query
                .OrderByDescending(p => p.SoldCount)
                .Take(maxCount)
                .Select(p => new ProductInfoDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryName = p.Category != null ? p.Category.Name : ""
                })
                .ToListAsync();
        }

        public async Task<List<ProductInfoDto>> GetPopularAsync(int count)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted && p.Stock > 0)
                .OrderByDescending(p => p.SoldCount)
                .Take(count)
                .Select(p => new ProductInfoDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryName = p.Category != null ? p.Category.Name : ""
                })
                .ToListAsync();
        }

        private static ProductDto MapToDto(TechStore.Domain.Entities.Product p)
        {
            return new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Price = p.Price,
                Stock = p.Stock,
                ShortDescription = p.ShortDescription,
                CategoryName = p.Category?.Name,
                BrandName = p.Brand?.Name,
                ImageUrl = p.Images?.FirstOrDefault(i => i.IsMain)?.ImageUrl
                    ?? p.Images?.FirstOrDefault()?.ImageUrl
            };
        }
    }
}