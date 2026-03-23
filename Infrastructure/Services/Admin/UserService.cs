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
                var orderStats = await _context.Orders
                    .Where(o => o.UserId == user.Id)
                    .GroupBy(o => o.UserId)
                    .Select(g => new
                    {
                        TotalOrders = g.Count(),
                        TotalSpent = g.Sum(o => (decimal?)o.TotalAmount) ?? 0
                    })
                    .FirstOrDefaultAsync();

                result.Add(new UserInfoDto
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    UserName = user.UserName ?? "",
                    TotalOrders = orderStats?.TotalOrders ?? 0,
                    TotalSpent = orderStats?.TotalSpent ?? 0
                });
            }

            return result;
        }

        public async Task<UserInfoDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var orderStats = await _context.Orders
                .Where(o => o.UserId == userId)
                .GroupBy(o => o.UserId)
                .Select(g => new
                {
                    TotalOrders = g.Count(),
                    TotalSpent = g.Sum(o => (decimal?)o.TotalAmount) ?? 0
                })
                .FirstOrDefaultAsync();

            return new UserInfoDto
            {
                UserId = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                TotalOrders = orderStats?.TotalOrders ?? 0,
                TotalSpent = orderStats?.TotalSpent ?? 0
            };
        }
    }
}
