using cybersoft_final_project.Models;
using cybersoft_final_project.Models.Request;
using Microsoft.AspNetCore.Mvc;
using SWD392_backend.Infrastructure.Services.OrderService;
using SWD392_backend.Models.Request;

namespace SWD392_backend.Infrastructure.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly IOrderService _orderService;

        public PaymentController(PaymentService paymentService, IOrderService orderService)
        {
            _paymentService = paymentService;
            _orderService = orderService;
        }

        /// <summary>
        /// Tạo một đơn hàng PayPal với tổng giá trị được cung cấp.
        /// </summary>
        /// <param name="totalPrice">Tổng số tiền thanh toán dạng chuỗi.</param>
        /// <returns>URL để người dùng xác nhận thanh toán PayPal.</returns>
        /// <response code="200">Trả về URL xác nhận thanh toán thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="500">Lỗi hệ thống khi tạo đơn hàng PayPal.</response>
        [HttpPost("paypal")]
        public async Task<IActionResult> CreatePaypalOrder([FromBody] string totalPrice)
        {
            try
            {
                var approvalUrl = await _paymentService.CreateOrderAsync(totalPrice);

                if (string.IsNullOrEmpty(approvalUrl))
                    return StatusCode(500, HTTPResponse<object>.Response(500, "Failed to create PayPal order.", null));

                return Ok(HTTPResponse<object>.Response(200, "PayPal order created successfully.", new { url = approvalUrl }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, HTTPResponse<object>.Response(500, "Internal server error", ex.Message));
            }
        }

        /// <summary>
        /// Thực hiện checkout đơn hàng dựa trên dữ liệu đơn hàng và user đang đăng nhập.
        /// </summary>
        /// <param name="orderCheckoutDto">Đối tượng chứa thông tin đơn hàng để thanh toán.</param>
        /// <returns>
        /// Kết quả tạo đơn hàng thành công hoặc lỗi chi tiết.
        /// </returns>
        /// <response code="200">Đơn hàng được tạo thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ hoặc UserId không hợp lệ.</response>
        /// <response code="401">User chưa đăng nhập hoặc không có quyền.</response>
        /// <response code="404">Không tìm thấy dữ liệu liên quan (ví dụ sản phẩm, user).</response>
        /// <response code="500">Lỗi hệ thống khi tạo đơn hàng.</response>
            [HttpPost("checkout")]
            public async Task<IActionResult> Checkout([FromBody] OrderCheckoutDTO orderCheckoutDto)
            {
                // Lấy userId từ claim, nếu không có trả về 401 Unauthorized
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(HTTPResponse<object>.Response(401, "UserId claim not found. Unauthorized.", null));

                if (!int.TryParse(userIdClaim, out int userId))
                    return BadRequest(HTTPResponse<object>.Response(400, "Invalid UserId claim format.", null));

                if (!ModelState.IsValid)
                    return BadRequest(HTTPResponse<object>.Response(400, "Invalid request data.", ModelState));

                try
                {
                    if (orderCheckoutDto.Distance < 1)
                    {
                        return BadRequest(HTTPResponse<object>.Response(400, "Distance must be at least 1 km.", null));
                    }
                    if (orderCheckoutDto.Distance > 1000)
                    {
                        return BadRequest(HTTPResponse<object>.Response(400, "Distance must not exceed 1000 km.", null));
                    }
                    var result = await _orderService.CheckoutAsync(orderCheckoutDto, userId);
                    if (result)
                        return Ok(HTTPResponse<object>.Response(200, "Order created successfully", null));
                    else
                        return StatusCode(500, HTTPResponse<object>.Response(500, "Failed to create order", null));
                }
                catch (ArgumentException argEx)
                {
                    // Ví dụ lỗi về dữ liệu đầu vào không hợp lệ
                    return BadRequest(HTTPResponse<object>.Response(400, argEx.Message, null));
                }
                catch (KeyNotFoundException keyEx)
                {
                    // Ví dụ lỗi do foreign key không tồn tại (sản phẩm, user, supplier ...)
                    return NotFound(HTTPResponse<object>.Response(404, keyEx.Message, null));
                }
                catch (Exception ex)
                {
                    return StatusCode(500, HTTPResponse<object>.Response(500, "Internal server error", ex.ToString()));
                }
            }
    }
}
