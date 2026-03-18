using Application.DTOs.Admin;
using Application.DTOs.Catalog;
using Application.DTOs.Integration;
using Application.DTOs.Orders;
using Application.Interfaces.Admin;
using Application.Interfaces.Catalog;
using Application.Interfaces.Integration;
using Application.Interfaces.Orders;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using TechStore.Domain.Entities;
using TechStore.Domain.Enums;
using TechStore.Infrastructure.Data;

namespace TechStore.Infrastructure.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> CreateReviewAsync(string userId, string userName, CreateReviewDto dto)
        {
            var productExists = await _context.Products
                .AnyAsync(p => p.Id == dto.ProductId && !p.IsDeleted);

            if (!productExists)
            {
                return (false, "Sản phẩm không tồn tại.");
            }

            var hasPurchased = await _context.OrderDetails
                .AnyAsync(od =>
                    od.ProductId == dto.ProductId &&
                    od.Order.UserId == userId &&
                    (od.Order.Status == OrderStatus.Confirmed ||
                     od.Order.Status == OrderStatus.Shipping ||
                     od.Order.Status == OrderStatus.Completed));

            if (!hasPurchased)
            {
                return (false, "Bạn chỉ có thể đánh giá sản phẩm đã mua.");
            }

            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.ProductId == dto.ProductId && r.UserId == userId && !r.IsDeleted);

            if (alreadyReviewed)
            {
                return (false, "Bạn đã đánh giá sản phẩm này rồi.");
            }

            var review = new Review
            {
                ProductId = dto.ProductId,
                UserId = userId,
                UserName = string.IsNullOrWhiteSpace(userName) ? "Người dùng" : userName,
                Rating = dto.Rating,
                Comment = dto.Comment
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return (true, "Đánh giá của bạn đã được ghi nhận.");
        }
    }
}