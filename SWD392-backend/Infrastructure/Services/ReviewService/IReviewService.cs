using SWD392_backend.Entities;
using SWD392_backend.Models;
using SWD392_backend.Models.Request;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Services.ReviewService
{
    public interface IReviewService
    {
        Task<PagedResult<ReviewResponse>> GetReviewsByProductIdAsync(int productId, int page = 1, int pageSize = 10);
        Task<ReviewResponse> AddReviewAsync(int userId, int productId, ReviewRequest request);
        Task<ReviewResponse?> UpdateReviewAsync(int userId, int productId, ReviewRequest request);
        Task<bool> RemoveReview(int userId, int productId);
    }
}
