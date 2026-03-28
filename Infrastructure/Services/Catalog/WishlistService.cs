using Application.DTOs.Catalog;
using Application.Interfaces.Catalog;
using Microsoft.EntityFrameworkCore;
using TechStore.Infrastructure.Data;

namespace TechStore.Infrastructure.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly ApplicationDbContext _context;

        public WishlistService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<WishlistItemDto>> GetItemsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new List<WishlistItemDto>();
            }

            return await _context.WishlistItems
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.UserId == userId && !x.Product.IsDeleted && x.Product.IsActive)
                .Include(x => x.Product)
                .ThenInclude(p => p.Images)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new WishlistItemDto
                {
                    WishlistItemId = x.Id,
                    ProductId = x.ProductId,
                    ProductName = x.Product.Name,
                    Price = x.Product.Price,
                    Stock = x.Product.Stock,
                    ImageUrl = x.Product.Images
                        .Where(i => !i.IsDeleted && i.IsMain)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault()
                        ?? x.Product.Images
                            .Where(i => !i.IsDeleted)
                            .Select(i => i.ImageUrl)
                            .FirstOrDefault()
                })
                .ToListAsync();
        }

        public async Task<HashSet<int>> GetProductIdsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new HashSet<int>();
            }

            var ids = await _context.WishlistItems
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.UserId == userId)
                .Select(x => x.ProductId)
                .ToListAsync();

            return ids.ToHashSet();
        }

        public async Task<(bool Success, string Message, int Count)> AddAsync(string userId, int productId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return (false, "Bạn cần đăng nhập để dùng wishlist.", 0);
            }

            var productExists = await _context.Products
                .AnyAsync(p => p.Id == productId && !p.IsDeleted && p.IsActive);

            if (!productExists)
            {
                var countInvalid = await GetCountAsync(userId);
                return (false, "Sản phẩm không tồn tại hoặc đã bị ẩn.", countInvalid);
            }

            var exists = await _context.WishlistItems
                .AnyAsync(x => !x.IsDeleted && x.UserId == userId && x.ProductId == productId);

            if (exists)
            {
                var countExists = await GetCountAsync(userId);
                return (false, "Sản phẩm đã có trong wishlist.", countExists);
            }

            _context.WishlistItems.Add(new Domain.Entities.WishlistItem
            {
                UserId = userId,
                ProductId = productId
            });

            await _context.SaveChangesAsync();
            var count = await GetCountAsync(userId);

            return (true, "Đã thêm vào wishlist.", count);
        }

        public async Task<(bool Success, string Message, int Count)> RemoveAsync(string userId, int productId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return (false, "Bạn cần đăng nhập để dùng wishlist.", 0);
            }

            var item = await _context.WishlistItems
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.UserId == userId && x.ProductId == productId);

            if (item == null)
            {
                var countNotFound = await GetCountAsync(userId);
                return (false, "Sản phẩm không có trong wishlist.", countNotFound);
            }

            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync();

            var count = await GetCountAsync(userId);
            return (true, "Đã xóa khỏi wishlist.", count);
        }

        public Task<int> GetCountAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Task.FromResult(0);
            }

            return _context.WishlistItems
                .AsNoTracking()
                .CountAsync(x => !x.IsDeleted && x.UserId == userId);
        }
    }
}