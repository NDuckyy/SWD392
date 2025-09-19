using Microsoft.EntityFrameworkCore;
using SWD392_backend.Context;
using SWD392_backend.Entities;
using SWD392_backend.Models;

namespace SWD392_backend.Infrastructure.Repositories.ReviewRepository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly MyDbContext _context;

        public ReviewRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddReviewAsync(product_review review)
        {
            await _context.product_reviews.AddAsync(review);
        }

        public void UpdateReviewAsync(product_review review)
        {
            _context.product_reviews.Update(review);
        }

        public async Task<product_review?> FindExistReviewAsync(int userId, int productId)
        {
            return await _context.product_reviews
                            .FirstOrDefaultAsync(r => r.UserId == userId 
                                                   && r.ProductId == productId);
        }

        public async Task<PagedResult<product_review>> GetReviewsByProductIdAsync(int productId, int page = 1, int pageSize = 10)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;

            // Total items
            var totalItems = await _context.product_reviews.Where(r => r.ProductId == productId).CountAsync();

            var reviews = await _context.product_reviews
                            .Where(r => r.ProductId == productId)
                            .Include(r => r.user)
                            .OrderByDescending(p => p.CreatedAt)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();

            return new PagedResult<product_review>
            {
                Items = reviews,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task LoadUserAsync(product_review review)
        {
            await _context.Entry(review).Reference(r => r.user).LoadAsync();
        }

        public void RemoveReview(product_review review)
        {
            _context.Remove(review);
        }
    }
}
