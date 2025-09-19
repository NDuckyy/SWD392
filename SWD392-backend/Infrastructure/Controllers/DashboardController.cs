using cybersoft_final_project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using SWD392_backend.Entities;
using SWD392_backend.Infrastructure.Services;
using SWD392_backend.Infrastructure.Services.OrderService;
using SWD392_backend.Infrastructure.Services.SupplerSerivce;
using SWD392_backend.Infrastructure.Services.UserService;

namespace SWD392_backend.Infrastructure.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly ISupplierService _supplierService;
        private readonly IDistributedCache _cache;

        public DashboardController(
            IUserService userService,
            IOrderService orderService,
            ISupplierService supplierService,
            IDistributedCache cache)
        {
            _userService = userService;
            _orderService = orderService;
            _supplierService = supplierService;
            _cache = cache;
        }

        /// <summary>
        /// Lấy số liệu thống kê về đơn hàng, người dùng và nhà cung cấp.
        /// </summary>
        /// <returns>Thông tin tổng quan cho dashboard.</returns>
        /// <response code="200">Trả về dữ liệu tổng quan thành công.</response>
        /// <response code="500">Lỗi hệ thống khi lấy dữ liệu tổng quan.</response>
        [HttpGet("overview")]
        public async Task<IActionResult> GetDashboardOverview()
        {
            try
            {
                string cacheKey = "dashboard:overview";
                var cachedData = await _cache.GetStringAsync(cacheKey);
                if (cachedData != null)
                {
                    var cachedResult = JsonSerializer.Deserialize<object>(cachedData);
                    return Ok(HTTPResponse<object>.Response(200, "Fetched dashboard data from cache", cachedResult));
                }

                var totalOrders = await _orderService.GetTotalOrdersAsync();
                var totalUsers = await _userService.GetTotalUserAsync();
                var totalSuppliers = await _supplierService.GetTotalSuppliersAsync();

                var dashboardData = new
                {
                    TotalOrders = totalOrders,
                    TotalUsers = totalUsers,
                    TotalSuppliers = totalSuppliers
                };

                var serializedData = JsonSerializer.Serialize(dashboardData);
                await _cache.SetStringAsync(cacheKey, serializedData, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) // cache 5 phút
                });

                return Ok(HTTPResponse<object>.Response(200, "Fetched dashboard data successfully", dashboardData));
            }
            catch (Exception ex)
            {
                return StatusCode(500, HTTPResponse<object>.Response(500, "Internal server error", ex.Message));
            }
        }
    }
}
