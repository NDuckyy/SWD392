using cybersoft_final_project.Models;
using cybersoft_final_project.Models.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using SWD392_backend.Context;
using SWD392_backend.Entities.Enums;
using SWD392_backend.Infrastructure.Services.OrderService;
using SWD392_backend.Models.Request;
using System.Text.Json;

namespace SWD392_backend.Infrastructure.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet()]
    public async Task<IActionResult> GetOrdersByRole([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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

            var result = await _orderService.GetOrdersByRoleAsync(role, id, page, pageSize);
            return Ok(HTTPResponse<object>.Response(200, "Fetched orders successfully.", result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, HTTPResponse<object>.Response(500, "Internal server error", ex.Message));
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateOrderItemStatus([FromBody] UpdateOrderItemStatus dto)
    {
        try
        {
            var updated = await _orderService.UpdateOrderItemStatusAsync(dto);
            if (updated)
                return Ok(HTTPResponse<object>.Response(200, "Order item status updated successfully.", null));
            else
                return Ok(HTTPResponse<object>.Response(200, "No change needed. Status is already up to date.", null));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(HTTPResponse<object>.Response(404, ex.Message, null));
        }
        catch (Exception ex)
        {
            return StatusCode(500, HTTPResponse<object>.Response(500, "Internal server error", ex.Message));
        }
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetEnumStatus([FromServices] IDistributedCache cache)
    {
        try
        {
            var role = User.FindFirst("Role")?.Value;
            if (string.IsNullOrEmpty(role))
                return Unauthorized(HTTPResponse<object>.Response(401, "Role claim not found.", null));

            string cacheKey = $"order:status:role:{role}";
            var cachedData = await cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                var cachedStatuses = JsonSerializer.Deserialize<List<string>>(cachedData);
                return Ok(HTTPResponse<List<string>>.Response(200, "Fetched statuses from cache successfully.",
                    cachedStatuses));
            }

            List<OrderStatus> allowedStatuses;

            if (role == "CUSTOMER")
            {
                allowedStatuses = new List<OrderStatus>
                {
                    OrderStatus.Pending,
                    OrderStatus.Preparing,
                    OrderStatus.Delivery,
                    OrderStatus.Delivered,
                    OrderStatus.Cancelled,
                    OrderStatus.Refunding,
                    OrderStatus.Refunded
                };
            }
            else if (role == "SHIPPER")
            {
                allowedStatuses = new List<OrderStatus>
                {
                    OrderStatus.Preparing,
                    OrderStatus.Delivery,
                    OrderStatus.Delivered
                };
            }
            else if (role == "SUPPLIER")
            {
                allowedStatuses = new List<OrderStatus>
                {
                    OrderStatus.Pending,
                    OrderStatus.Preparing,
                    OrderStatus.Cancelled,
                    OrderStatus.Refunding
                };
            }
            else
            {
                return Unauthorized(HTTPResponse<object>.Response(401, "Unsupported role.", null));
            }

            var result = allowedStatuses.Select(s => s.ToString()).ToList();

            var serialized = JsonSerializer.Serialize(result);
            await cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
            });

            return Ok(HTTPResponse<List<string>>.Response(200, "Fetched statuses successfully.", result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, HTTPResponse<object>.Response(500, "Internal server error", ex.Message));
        }
    }

    [HttpPut("update-status")]
    public async Task<IActionResult> UpdateStatus(string orderId, OrderStatus status)
    {
        try
        {
            if (orderId.IsNullOrEmpty())
                return BadRequest("Invalid orderId or productId.");

            await _orderService.UpdateOrderStatus(orderId, status);

            return Ok(new
            {
                message = "Order detail status updated successfully.",
                orderId,
                newStatus = status.ToString()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}