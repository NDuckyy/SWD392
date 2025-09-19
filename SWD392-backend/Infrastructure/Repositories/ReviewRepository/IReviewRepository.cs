using SWD392_backend.Entities;
using SWD392_backend.Models;

namespace SWD392_backend.Infrastructure.Repositories.ReviewRepository
{
    public interface IReviewRepository
    {
        Task AddReviewAsync(product_review review);
        Task LoadUserAsync(product_review review);
        Task<PagedResult<product_review>> GetReviewsByProductIdAsync(int productId, int page = 1, int pageSize = 10);
        Task<product_review> FindExistReviewAsync(int userId, int productId);
        void UpdateReviewAsync(product_review review);
        void RemoveReview(product_review review);
    }
}
