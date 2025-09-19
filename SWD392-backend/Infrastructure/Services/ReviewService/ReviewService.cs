using AutoMapper;
using Elastic.Clients.Elasticsearch.Security;
using SWD392_backend.Entities;
using SWD392_backend.Infrastructure.Repositories.ReviewRepository;
using SWD392_backend.Infrastructure.Services.UserService;
using SWD392_backend.Models;
using SWD392_backend.Models.Request;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Services.ReviewService
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public ReviewService(IReviewRepository reviewRepository, IUnitOfWork unitOfWork, IMapper mapper, IUserService userService)
        {
            _reviewRepository = reviewRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<PagedResult<ReviewResponse>> GetReviewsByProductIdAsync(int productId, int page = 1, int pageSize = 10)
        {
            var pagedResult = await _reviewRepository.GetReviewsByProductIdAsync(productId, page, pageSize);

            var response = _mapper.Map<List<ReviewResponse>>(pagedResult.Items);

            return new PagedResult<ReviewResponse>
            {
                Items = response,
                TotalItems = pagedResult.TotalItems,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<ReviewResponse> AddReviewAsync(int userId, int productId, ReviewRequest request)
        {
            // Check exist review from user
            var existingReview = await _reviewRepository.FindExistReviewAsync(userId, productId);
            if (existingReview != null)
                return null;

            var review = _mapper.Map<product_review>(request);
            review.UserId = userId;
            review.ProductId = productId;
            review.CreatedAt = DateTime.UtcNow;

            // Add
            await _reviewRepository.AddReviewAsync(review);

            // Save to DB
            await _unitOfWork.SaveAsync();

            // Load user
            await _reviewRepository.LoadUserAsync(review);

            var response = _mapper.Map<ReviewResponse>(review);

            return response;
        }

        public async Task<ReviewResponse?> UpdateReviewAsync(int userId, int productId, ReviewRequest request)
        {
            var review = await _reviewRepository.FindExistReviewAsync(userId, productId);
            review.Content = request.Content;
            review.Rating = request.Rating;
            review.CreatedAt = DateTime.UtcNow;

            // update
            _reviewRepository.UpdateReviewAsync(review);

            // Save to DB
            await _unitOfWork.SaveAsync();

            // Load user
            await _reviewRepository.LoadUserAsync(review);

            var response = _mapper.Map<ReviewResponse>(review);

            return response;
        }

        public async Task<bool> RemoveReview(int userId, int productId)
        {
            var existingReview = await _reviewRepository.FindExistReviewAsync(userId, productId);
            if (existingReview == null)
                return false;
            
            // Remove 
            _reviewRepository.RemoveReview(existingReview);

            // Save to DB
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}
