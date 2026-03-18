using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;
using Application.DTOs;

namespace Application.Interfaces.Admin {
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync();
    }
}