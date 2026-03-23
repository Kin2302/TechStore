using Application.DTOs.Admin;

namespace Application.Interfaces.Admin
{
    public interface IUserService
    {
        Task<List<UserInfoDto>> SearchUsersAsync(string query);
        Task<UserInfoDto?> GetUserByIdAsync(string userId);
    }
}
