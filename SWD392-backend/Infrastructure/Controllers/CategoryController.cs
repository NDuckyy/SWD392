using cybersoft_final_project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using SWD392_backend.Entities;
using SWD392_backend.Infrastructure.Services.CategoryService;
using SWD392_backend.Models.Response;
using System.Text.Json;
using StackExchange.Redis;

namespace SWD392_backend.Infrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;

        public CategoryController(ICategoryService categoryService, IDistributedCache cache, IConnectionMultiplexer redis)
        {
            _categoryService = categoryService;
            _cache = cache;
            _redis = redis;
        }

        /// <summary>
        /// Lấy danh sách tất cả các danh mục (categories).
        /// </summary>
        /// <returns>Danh sách các category dưới dạng mảng JSON.</returns>
        /// <response code="200">Trả về danh sách category thành công.</response>
        /// <response code="400">Không tìm thấy category nào hoặc có lỗi trong quá trình xử lý.</response>
        [HttpGet]
        public async Task<ActionResult<List<CategoryResponse>>> GetCategories()
        {
            const string cacheKey = "categories:all";
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (cachedData != null)
            {
                var cachedResult = JsonSerializer.Deserialize<List<CategoryResponse>>(cachedData);
                return Ok(HTTPResponse<List<CategoryResponse>>.Response(200, "Lấy category từ cache", cachedResult));
            }

            var response = await _categoryService.GetCategoriesAsync();

            if (response == null)
                return BadRequest(HTTPResponse<object>.Response(400, "Không tìm thấy category nào", null));

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            });

            return Ok(HTTPResponse<List<CategoryResponse>>.Response(200, "Lấy toàn bộ category thành công", response));
        }
    }
}
