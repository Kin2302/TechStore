using Application.DTOs;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using TechStore.Domain.Entities;
using TechStore.Infrastructure.Data;

namespace Infrastructure.Services
{
    public class AdminCategoryService : IAdminCategoryService
    {
        private readonly ApplicationDbContext _context;

        public AdminCategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted)
                .Include(c => c.Parent)
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    IconUrl = c.IconUrl,
                    ParentId = c.ParentId
                })
                .ToListAsync();
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .Where(c => c.Id == id && !c.IsDeleted)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    IconUrl = c.IconUrl,
                    ParentId = c.ParentId
                })
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(CategoryDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                Slug = dto.Slug ?? GenerateSlug(dto.Name),
                Description = dto.Description,
                IconUrl = dto.IconUrl,
                ParentId = dto.ParentId
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CategoryDto dto)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == dto.Id && !c.IsDeleted);

            if (category == null) return;

            category.Name = dto.Name;
            category.Slug = dto.Slug ?? GenerateSlug(dto.Name);
            category.Description = dto.Description;
            category.IconUrl = dto.IconUrl;
            category.ParentId = dto.ParentId;
            category.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return;

            category.IsDeleted = true;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CategoryDto>> GetParentCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted && c.ParentId == null)
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
        }

        private static string GenerateSlug(string name)
        {
            return name.ToLower()
                .Replace(" ", "-")
                .Replace(".", "")
                .Replace(",", "");
        }
    }
}