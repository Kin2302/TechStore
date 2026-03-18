using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using TechStore.Domain.Entities;

namespace Application.Interfaces.Admin {
    public interface IAdminProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync(string? search, int? categoryId);
        Task<ProductEditDto?> GetByIdForEditAsync(int id);
        Task CreateAsync(ProductCreateDto dto, string? imageUrl);
        Task UpdateAsync(ProductEditDto dto, string? newImageUrl);
        Task DeleteAsync(int id);
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<IEnumerable<Brand>> GetBrandsAsync();
    }
}
