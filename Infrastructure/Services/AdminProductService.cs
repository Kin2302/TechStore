using Application.DTOs;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechStore.Domain.Entities;
using TechStore.Infrastructure.Data;

namespace TechStore.Infrastructure.Services
{
    public class AdminProductService : IAdminProductService
    {
        private readonly ApplicationDbContext _context;
        public AdminProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(ProductCreateDto dto, string? imageUrl)
        {
            var slug = dto.Name
                .ToLower()
                .Replace(" ", "-")
                .Replace(".", "")
                .Replace(",", "");

            var product = new Product
            {
                Name = dto.Name,
                Code = dto.Code,
                ShortDescription = dto.ShortDescription,
                Description = dto.Description,
                Price = dto.Price,
                DiscountPrice = dto.DiscountPrice,
                Stock = dto.Stock,
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,
                IsActive = dto.IsActive,
                IsFeatured = dto.IsFeatured,
                Slug = slug
            };

            if (imageUrl != null)
            {
                var image = new ProductImage
                {
                    ImageUrl = imageUrl,
                    IsMain = true
                };
                product.Images.Add(image);
            }

            // Thêm specs TRƯỚC SaveChanges
            foreach (var spec in dto.Specifications.Where(s => !string.IsNullOrEmpty(s.Name)))
            {
                product.Specifications.Add(new ProductSpecification
                {
                    Name = spec.Name,
                    Value = spec.Value
                });
            }

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();  // ← Lưu product + images + specs cùng lúc
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(string? search, int? categoryId)
        {
            var query = _context.Products.Where(p => !p.IsDeleted);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Code.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await query
                .OrderByDescending(p => p.CreatedDate)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    Price = p.Price,
                    Stock = p.Stock,
                    ImageUrl = p.Images.Where(i => i.IsMain).Select(i => i.ImageUrl).FirstOrDefault(),
                    CategoryName = p.Category.Name,
                    BrandName = p.Brand != null ? p.Brand.Name : null,
                    ShortDescription = p.ShortDescription
                })
                .ToListAsync();

            return products;
        }

        public async Task<IEnumerable<Brand>> GetBrandsAsync()
        {
            return await _context.Brands.ToListAsync();
        }

        public async Task<ProductEditDto?> GetByIdForEditAsync(int id)
        {
            return await _context.Products
                .Where(p => p.Id == id && !p.IsDeleted)
                .Select(p => new ProductEditDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    ShortDescription = p.ShortDescription,
                    Description = p.Description,
                    Price = p.Price,
                    DiscountPrice = p.DiscountPrice,
                    Stock = p.Stock,
                    CategoryId = p.CategoryId,
                    BrandId = p.BrandId,
                    IsActive = p.IsActive,
                    IsFeatured = p.IsFeatured,
                    ExistingImageUrl = p.Images.Where(i => i.IsMain).Select(i => i.ImageUrl).FirstOrDefault(),
                    Specifications = p.Specifications.Select(s => new SpecificationInputDto
                    {
                        Name = s.Name,
                        Value = s.Value
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task UpdateAsync(ProductEditDto dto, string? newImageUrl)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Specifications)  // ← THÊM
                .FirstOrDefaultAsync(p => p.Id == dto.Id && !p.IsDeleted);

            if (product == null) return;

            product.Name = dto.Name;
            product.Code = dto.Code;
            product.ShortDescription = dto.ShortDescription;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.DiscountPrice = dto.DiscountPrice;
            product.Stock = dto.Stock;
            product.CategoryId = dto.CategoryId;
            product.BrandId = dto.BrandId;
            product.IsActive = dto.IsActive;
            product.IsFeatured = dto.IsFeatured;
            product.Slug = dto.Name.ToLower()
                .Replace(" ", "-")
                .Replace(".", "")
                .Replace(",", "");
            product.UpdatedDate = DateTime.UtcNow;

            if (newImageUrl != null)
            {
                var existingMainImage = product.Images.FirstOrDefault(i => i.IsMain);
                if (existingMainImage != null)
                {
                    existingMainImage.ImageUrl = newImageUrl;
                }
                else
                {
                    product.Images.Add(new ProductImage
                    {
                        ImageUrl = newImageUrl,
                        IsMain = true
                    });
                }
            }

            // Xóa specs cũ
            product.Specifications.Clear();

            // Thêm specs mới
            foreach (var spec in dto.Specifications.Where(s => !string.IsNullOrEmpty(s.Name)))
            {
                product.Specifications.Add(new ProductSpecification
                {
                    Name = spec.Name,
                    Value = spec.Value
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
