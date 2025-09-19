using cybersoft_final_project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using SWD392_backend.Entities;
using SWD392_backend.Infrastructure.Services.ReviewService;
using SWD392_backend.Models;
using SWD392_backend.Models.Request;
using SWD392_backend.Models.Response;
using System.Text.Json;

namespace SWD392_backend.Infrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly IDistributedCache _cache;

        public ReviewController(IReviewService reviewService, IDistributedCache cache)
        {
            _reviewService = reviewService;
            _cache = cache;
        }

        [HttpGet("all")]
        public async Task<ActionResult<PagedResult<ReviewResponse>>> GetAllReviews([FromQuery] int productId, int page = 1, int pageSize = 10)
        {
            string cacheKey = $"reviews:product:{productId}:page:{page}:size:{pageSize}";
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                var cachedResult = JsonSerializer.Deserialize<PagedResult<ReviewResponse>>(cachedData);
                return Ok(HTTPResponse<object>.Response(200, "Lấy đánh giá từ cache", cachedResult));
            }

            var response = await _reviewService.GetReviewsByProductIdAsync(productId, page, pageSize);
            if (response.Items == null || !response.Items.Any())
                return BadRequest(HTTPResponse<object>.Response(400, "Lấy đánh giá thất bại", response));

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

            return Ok(HTTPResponse<object>.Response(200, "Lấy đánh giá thành công", response));
        }

        [HttpPost("add")]
        public async Task<ActionResult<ReviewResponse>> AddReview([FromQuery] int productId, [FromBody] ReviewRequest request)
        {
            try
            {
                var role = User.FindFirst("Role")?.Value;
                if (string.IsNullOrEmpty(role)) return Unauthorized(HTTPResponse<object>.Response(401, "Role claim not found.", null));

                string? idClaimType = role == "CUSTOMER" ? "UserId" : role == "SUPPLIER" ? "SupplierId" : null;
                if (idClaimType == null) return Unauthorized(HTTPResponse<object>.Response(401, "Unsupported role.", null));

                var idClaim = User.FindFirst(idClaimType)?.Value;
                if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int id))
                    return BadRequest(HTTPResponse<object>.Response(400, $"Invalid or missing {idClaimType}.", null));

                var response = await _reviewService.AddReviewAsync(id, productId, request);
                if (response == null)
                    return BadRequest(HTTPResponse<object>.Response(400, "Thêm đánh giá thất bại", null));

                // Xóa cache sau khi thêm
                await InvalidateReviewCacheAsync(productId);

                return Ok(HTTPResponse<ReviewResponse>.Response(200, "Thêm đánh giá thành công", response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, HTTPResponse<object>.Response(500, "Internal server error", ex.Message));
            }
        }

        [HttpPut("update")]
        public async Task<ActionResult<ReviewResponse>> UpdateReview([FromQuery] int productId, [FromBody] ReviewRequest request)
        {
            try
            {
                var role = User.FindFirst("Role")?.Value;
                if (string.IsNullOrEmpty(role)) return Unauthorized(HTTPResponse<object>.Response(401, "Role claim not found.", null));

                string? idClaimType = role == "CUSTOMER" ? "UserId" : role == "SUPPLIER" ? "SupplierId" : null;
                if (idClaimType == null) return Unauthorized(HTTPResponse<object>.Response(401, "Unsupported role.", null));

                var idClaim = User.FindFirst(idClaimType)?.Value;
                if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int id))
                    return BadRequest(HTTPResponse<object>.Response(400, $"Invalid or missing {idClaimType}.", null));

                var response = await _reviewService.UpdateReviewAsync(id, productId, request);
                if (response == null)
                    return BadRequest(HTTPResponse<object>.Response(400, "Cập nhật đánh giá thất bại", null));

                // Xóa cache sau khi cập nhật
                await InvalidateReviewCacheAsync(productId);

                return Ok(HTTPResponse<ReviewResponse>.Response(200, "Cập nhật đánh giá thành công", response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, HTTPResponse<object>.Response(500, "Internal server error", ex.Message));
            }
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveReview([FromQuery] int productId)
        {
            try
            {
                var role = User.FindFirst("Role")?.Value;
                if (string.IsNullOrEmpty(role)) return Unauthorized(HTTPResponse<object>.Response(401, "Role claim not found.", null));

                string? idClaimType = role == "CUSTOMER" ? "UserId" : role == "SUPPLIER" ? "SupplierId" : null;
                if (idClaimType == null) return Unauthorized(HTTPResponse<object>.Response(401, "Unsupported role.", null));

                var idClaim = User.FindFirst(idClaimType)?.Value;
                if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int id))
                    return BadRequest(HTTPResponse<object>.Response(400, $"Invalid or missing {idClaimType}.", null));

                var response = await _reviewService.RemoveReview(id, productId);
                if (!response)
                    return BadRequest(HTTPResponse<object>.Response(400, "Xóa đánh giá thất bại", false));

                // Xóa cache sau khi xóa
                await InvalidateReviewCacheAsync(productId);

                return Ok(HTTPResponse<object>.Response(200, "Xóa đánh giá thành công", true));
            }
            catch (Exception ex)
            {
                return StatusCode(500, HTTPResponse<object>.Response(500, "Internal server error", ex.Message));
            }
        }

        private async Task InvalidateReviewCacheAsync(int productId)
        {
            // Duyệt các page phổ biến để xóa cache
            for (int page = 1; page <= 3; page++)
            {
                string key = $"reviews:product:{productId}:page:{page}:size:10";
                await _cache.RemoveAsync(key);
            }
        }
    }
}
