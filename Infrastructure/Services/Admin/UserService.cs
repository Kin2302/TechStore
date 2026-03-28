using Application.DTOs.Admin;
using Application.Interfaces.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechStore.Infrastructure.Data;

namespace TechStore.Infrastructure.Services.Admin
{
    public class UserService : IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserService(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<List<UserInfoDto>> SearchUsersAsync(string query)
        {
            var lowerQuery = query.ToLower();
            var users = await _userManager.Users
                .Where(u => u.Email != null && (
                    u.Email.ToLower().Contains(lowerQuery) ||
                    (u.UserName != null && u.UserName.ToLower().Contains(lowerQuery))
                ))
                .Take(10)
                .ToListAsync();

            var result = new List<UserInfoDto>();
            foreach (var user in users)
            {
                result.Add(await BuildUserInfoAsync(user));
            }

            return result;
        }

        public async Task<List<UserInfoDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            var result = new List<UserInfoDto>();
            foreach (var user in users)
            {
                result.Add(await BuildUserInfoAsync(user));
            }

            return result;
        }

        public async Task<UserInfoDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return await BuildUserInfoAsync(user);
        }

        public async Task<bool> UpdateUserRoleAsync(string userId, string role)
        {
            if (string.IsNullOrWhiteSpace(userId)) return false;
            if (role != "Admin" && role != "User") return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Contains(role)) return true;

            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded) return false;
            }

            var addResult = await _userManager.AddToRoleAsync(user, role);
            return addResult.Succeeded;
        }

        private async Task<UserInfoDto> BuildUserInfoAsync(IdentityUser user)
        {
            var orderStats = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .GroupBy(o => o.UserId)
                .Select(g => new
                {
                    TotalOrders = g.Count(),
                    TotalSpent = g.Sum(o => (decimal?)o.TotalAmount) ?? 0
                })
                .FirstOrDefaultAsync();

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.Contains("Admin") ? "Admin" : "User";

            return new UserInfoDto
            {
                UserId = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                Role = role,
                TotalOrders = orderStats?.TotalOrders ?? 0,
                TotalSpent = orderStats?.TotalSpent ?? 0
            };
        }
    }
}
