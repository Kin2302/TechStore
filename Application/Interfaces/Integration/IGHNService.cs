using Application.DTOs.Integration;

namespace Application.Interfaces.Integration
{
    public interface IGHNService
    {
        Task<bool> PingAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<GHNProvinceDto>> GetProvincesAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<GHNDistrictDto>> GetDistrictsAsync(int provinceId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<GHNWardDto>> GetWardsAsync(int districtId, CancellationToken cancellationToken = default);

        Task<int?> GetShippingFeeAsync(
            int toDistrictId,
            string toWardCode,
            int insuranceValue,
            CancellationToken cancellationToken = default);

        Task<GHNCreateShipmentResultDto?> CreateShipmentAsync(
            GHNCreateShipmentRequestDto request,
            CancellationToken cancellationToken = default);

        Task<GHNShipmentDetailDto?> GetShipmentDetailAsync(
            string shippingCode,
            CancellationToken cancellationToken = default);
    }
}