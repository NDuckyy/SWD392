using cybersoft_final_project.Models;
using Elastic.Clients.Elasticsearch.Inference;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWD392_backend.Infrastructure.Services.OrderService;
using SWD392_backend.Models;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public ReportController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("ordersbymonth")]
        public async Task<ActionResult<ReportOrderResponse>> GetOrdersByMonth([FromQuery] int month, [FromQuery] int year, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _orderService.CountOrdersByMonthAsync(month, year, pageNumber, pageSize);
                return Ok(HTTPResponse<object>.Response(200, "Get Succesfully", result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(HTTPResponse<object>.Response(400, ex.Message, null));
            }
            catch (Exception ex)
            {
                return BadRequest(HTTPResponse<object>.Response(400, ex.Message, null));
            }
        }

        [HttpGet("ordersbyday")]
        public async Task<IActionResult> GetOrdersByMonth([FromQuery] int day, [FromQuery] int month, [FromQuery] int year, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _orderService.CountOrdersByDayAsync(day, month, year, pageNumber, pageSize);
                return Ok(HTTPResponse<object>.Response(200, "Get Succesfully", result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("summarythismonth")]
        public async Task<IActionResult> SummaryThisMonth()
        {
            try
            {
                (int totalOrders, int totalUsers) = await _orderService.GetSummaryThisMonthAsync();
                return Ok(new
                {
                    totalOrders,
                    totalUsers
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }
}
