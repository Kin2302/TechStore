using Application.DTOs;

namespace Application.Interfaces
{
    public interface IReviewService
    {
        Task<(bool Success, string Message)> CreateReviewAsync(string userId, string userName, CreateReviewDto dto);
    }
}