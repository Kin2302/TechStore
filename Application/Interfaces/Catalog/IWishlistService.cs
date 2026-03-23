using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Catalog;

namespace Application.Interfaces.Catalog
{
    public interface IWishlistService
    {
        List<int> GetWishlistProductIds();
        Task<List<CompareItemDto>> GetWishlistProductsAsync();
        Task<(bool Success, string Message, int Count)> AddProductAsync(int productId);
        int RemoveProduct(int productId);
        void Clear();
        int GetCount();
    }
}
