using Application.DTOs.Catalog;

namespace Application.Interfaces.Catalog
{
    public interface IWishlistService
    {
        Task<List<WishlistItemDto>> GetItemsAsync(string userId);
        Task<HashSet<int>> GetProductIdsAsync(string userId);
        Task<(bool Success, string Message, int Count)> AddAsync(string userId, int productId);
        Task<(bool Success, string Message, int Count)> RemoveAsync(string userId, int productId);
        Task<int> GetCountAsync(string userId);
    }
}