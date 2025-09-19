using System.Text.Json;
using AutoMapper;
using cybersoft_final_project.Models;
using cybersoft_final_project.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Win32.SafeHandles;
using SWD392_backend.Entities;
using SWD392_backend.Infrastructure.Services.ElasticSearchService;
using SWD392_backend.Infrastructure.Services.ProductService;
using SWD392_backend.Models;
using SWD392_backend.Models.Request;
using SWD392_backend.Models.Response;
using StackExchange.Redis;
using Elastic.Clients.Elasticsearch.Requests;

namespace SWD392_backend.Infrastructure.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IElasticSearchService _elasticsearchService;
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;

        public ProductController(IProductService productService, IElasticSearchService elasticSearchService,
            IDistributedCache cache, IConnectionMultiplexer redis)
        {
            _productService = productService;
            _elasticsearchService = elasticSearchService;
            _cache = cache;
            _redis = redis;
        }

        private async Task ClearProductListCacheAsync()
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            foreach (var key in server.Keys(pattern: "products:*"))
            {
                await _cache.RemoveAsync(key);
            }
        }

        /// <summary>
        /// Search product
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<List<ProductResponse>>> SearchProduct(
            [FromQuery] string q = "",
            [FromQuery] int? categoryId = null,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] string sortBy = "createdAt",
            [FromQuery] string sortOrder = "desc")
        {
            var response = await _elasticsearchService.SearchAsync(q, categoryId, page, size, sortBy, sortOrder);

            if (response == null)
                return NotFound();
            else
                return Ok(response);
        }

        /// <summary>
        /// Get list product
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<ProductResponse>>> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            string cacheKey = $"products:page:{page}:size:{pageSize}";
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                var cachedResult = JsonSerializer.Deserialize<PagedResult<ProductResponse>>(cachedData);
                return Ok(HTTPResponse<object>.Response(200, "Lấy list product từ cache", cachedResult));
            }

            var products = await _productService.GetPagedProductAsync(page, pageSize);

            var serializedData = JsonSerializer.Serialize(products);
            await _cache.SetStringAsync(cacheKey, serializedData, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
            });

            return Ok(HTTPResponse<object>.Response(200, "Lấy list product thành công", products));
        }


        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDetailResponse>> GetById(int id)
        {
            string cacheKey = $"products:id:{id}";
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                var cachedResult = JsonSerializer.Deserialize<ProductDetailResponse>(cachedData);
                return Ok(HTTPResponse<object>.Response(200, "Lấy sản phẩm thành công", cachedResult));
            }

            var products = await _productService.GetByIdAsync(id);
            if (products == null)
                return BadRequest(HTTPResponse<object>.Response(400, "Không có sản phẩm trùng khớp", null));

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(products), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            });

            return Ok(HTTPResponse<object>.Response(200, "Lấy sản phẩm thành công", products));
        }

        /// <summary>
        /// Get product by slug
        /// </summary>
        [HttpGet("{slug}")]
        public async Task<ActionResult<ProductDetailResponse>> GetBySlug(string slug)
        {
            //string cacheKey = $"products:slug:{slug}";
            //var cachedData = await _cache.GetStringAsync(cacheKey);
            //if (cachedData != null)
            //{
            //    var cachedResult = JsonSerializer.Deserialize<ProductDetailResponse>(cachedData);
            //    return Ok(HTTPResponse<object>.Response(200, "Lấy sản phẩm từ cache", cachedResult));
            //}

            var products = await _productService.GetBySlugAsync(slug);
            if (products == null)
                return BadRequest(HTTPResponse<object>.Response(400, "Không có sản phẩm trùng khớp", null));

            //await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(products), new DistributedCacheEntryOptions
            //{
            //    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            //});

            return Ok(HTTPResponse<object>.Response(200, "Lấy sản phẩm thành công", products));
        }

        /// <summary>
        /// Add product
        /// </summary>
        [HttpPost("add")]
        public async Task<ActionResult<ProductResponse>> AddProduct([FromBody] AddProductRequest request)
        {
            try
            {
                var role = User.FindFirst("Role")?.Value;
                if (string.IsNullOrEmpty(role))
                    return Unauthorized(HTTPResponse<object>.Response(401, "Role claim not found.", null));

                string? idClaimType = role == "CUSTOMER" ? "UserId" : role == "SUPPLIER" ? "SupplierId" : null;
                if (idClaimType == null)
                    return Unauthorized(HTTPResponse<object>.Response(401, "Unsupported role.", null));

                var idClaim = User.FindFirst(idClaimType)?.Value;
                if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int id))
                    return BadRequest(HTTPResponse<object>.Response(400, $"Invalid or missing {idClaimType}.", null));

                var result = await _productService.AddProductAsync(id, request);

                await ClearProductListCacheAsync();
                return Ok(HTTPResponse<object>.Response(200, "Thêm sản phẩm thành công", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, HTTPResponse<object>.Response(500, "Internal server error", ex.Message));
            }
        }

        /// <summary>
        /// Update product
        /// </summary>
        [HttpPost("update/{productId}")]
        public async Task<ActionResult<ProductResponse>> UpdateProduct(int productId,
            [FromBody] UpdateProductRequest request)
        {
            try
            {
                var role = User.FindFirst("Role")?.Value;
                if (string.IsNullOrEmpty(role))
                    return Unauthorized(HTTPResponse<object>.Response(401, "Role claim not found.", null));

                string? idClaimType = role == "CUSTOMER" ? "UserId" : role == "SUPPLIER" ? "SupplierId" : null;
                if (idClaimType == null)
                    return Unauthorized(HTTPResponse<object>.Response(401, "Unsupported role.", null));

                var idClaim = User.FindFirst(idClaimType)?.Value;
                if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int id))
                    return BadRequest(HTTPResponse<object>.Response(400, $"Invalid or missing {idClaimType}.", null));

                var result = await _productService.UpdateProductAsync(id, productId, request);
                if (result == null)
                    return BadRequest(HTTPResponse<object>.Response(400, "Cập nhật sản phẩm thất bại", null));

                await _cache.RemoveAsync($"products:id:{productId}");
                await ClearProductListCacheAsync();

                return Ok(HTTPResponse<object>.Response(200, "Cập nhật sản phẩm thành công", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, HTTPResponse<object>.Response(500, "Internal server error", ex.Message));
            }
        }

        /// <summary>
        /// Disable product
        /// </summary>
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> StatusProduct(int id, [FromBody] UpdateStatusProductRequest request)
        {
            var response = await _productService.UpdateProductStatusAsync(id, request);
            if (!response)
                return BadRequest(HTTPResponse<object>.Response(400, "Cập nhật sản phẩm thất bại", null));

            await _cache.RemoveAsync($"products:id:{id}");
            await ClearProductListCacheAsync();

            return Ok(HTTPResponse<object>.Response(200, "Cập nhật sản phẩm thành công", response));
        }

        /// <summary>
        /// Delete product
        /// </summary>
        [HttpDelete("{productId:int}")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            try
            {
                var role = User.FindFirst("Role")?.Value;
                if (string.IsNullOrEmpty(role))
                    return Unauthorized(HTTPResponse<object>.Response(401, "Role claim not found.", null));

                string? idClaimType = role == "CUSTOMER" ? "UserId" : role == "SUPPLIER" ? "SupplierId" : null;
                if (idClaimType == null)
                    return Unauthorized(HTTPResponse<object>.Response(401, "Unsupported role.", null));

                var idClaim = User.FindFirst(idClaimType)?.Value;
                if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int id))
                    return BadRequest(HTTPResponse<object>.Response(400, $"Invalid or missing {idClaimType}.", null));

                var result = await _productService.RemoveProductStatusAsync(id, productId);
                if (!result)
                    return BadRequest(HTTPResponse<object>.Response(400, "Xóa sản phẩm thất bại", null));

                await _cache.RemoveAsync($"products:id:{productId}");
                await ClearProductListCacheAsync();

                return Ok(HTTPResponse<object>.Response(200, "Xóa sản phẩm thành công", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, HTTPResponse<object>.Response(500, "Internal server error", ex.Message));
            }
        }
    }
}