using Application.DTOs;

namespace Application.Interfaces
{
    public interface ICompareService
    {
        List<int> GetCompareProductIds();
        Task<List<CompareItemDto>> GetCompareProductsAsync();
        Task<(bool Success, string Message, int Count)> AddProductAsync(int productId);
        int RemoveProduct(int productId);
        void Clear();
        int GetCount();
    }
}