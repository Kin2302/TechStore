using Application.DTOs;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using TechStore.Domain.Entities;
using TechStore.Infrastructure.Data;

namespace Infrastructure.Services
{
    public class AdminBrandService : IAdminBrandService
    {
        private readonly ApplicationDbContext _context;

        public AdminBrandService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BrandDto>> GetAllAsync()
        {
            return await _context.Brands
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.Name)
                .Select(b => new BrandDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Origin = b.Origin,
                    LogoUrl = b.LogoUrl,
                    ProductCount = b.Products.Count(p => !p.IsDeleted)
                })
                .ToListAsync();
        }

        public async Task<BrandDto?> GetByIdAsync(int id)
        {
            return await _context.Brands
                .Where(b => b.Id == id && !b.IsDeleted)
                .Select(b => new BrandDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Origin = b.Origin,
                    LogoUrl = b.LogoUrl
                })
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(BrandDto dto, string? logoUrl)
        {
            var brand = new Brand
            {
                Name = dto.Name,
                Origin = dto.Origin,
                LogoUrl = logoUrl
            };

            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BrandDto dto, string? newLogoUrl)
        {
            var brand = await _context.Brands
                .FirstOrDefaultAsync(b => b.Id == dto.Id && !b.IsDeleted);

            if (brand == null) return;

            brand.Name = dto.Name;
            brand.Origin = dto.Origin;
            brand.UpdatedDate = DateTime.UtcNow;

            if (newLogoUrl != null)
                brand.LogoUrl = newLogoUrl;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return;

            brand.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
}