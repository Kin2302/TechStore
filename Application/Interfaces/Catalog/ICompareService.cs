using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;
using Application.DTOs;

namespace Application.Interfaces.Catalog {
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