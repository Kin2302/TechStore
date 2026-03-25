using Application.DTOs.Admin;

namespace Application.Interfaces.Admin
{
    public interface IUserService
    {
        Task<List<UserInfoDto>> SearchUsersAsync(string query);
        Task<List<UserInfoDto>> GetAllUsersAsync();
        Task<UserInfoDto?> GetUserByIdAsync(string userId);
        Task<bool> UpdateUserRoleAsync(string userId, string role);
    }
}
    