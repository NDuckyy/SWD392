using cybersoft_final_project.Models;
using Elastic.Clients.Elasticsearch.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWD392_backend.Infrastructure.Services.AuthService;
using SWD392_backend.Infrastructure.Services.SupplerSerivce;
using SWD392_backend.Models;
using SWD392_backend.Models.Request;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        private readonly IAuthService _authService;

        public SupplierController(ISupplierService supplierService, IAuthService authService)
        {
            _supplierService = supplierService;
            _authService = authService;
        }

        /// <summary>
        /// List products for SUPPLIER
        /// </summary>
        [HttpGet("products")]
        public async Task<ActionResult<PagedResult<ProductResponse>>> GetListProducts(int pageNumber = 1, int pageSize = 10)
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

                var result = await _supplierService.GetPagedProductsAsync(id, pageNumber, pageSize);

                return Ok(HTTPResponse<object>.Response(200, "Lấy danh sách sản phẩm thành công", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, HTTPResponse<object>.Response(500, "Internal server error", ex.Message));
            }
        }

        /// <summary>
        /// Get product by ID for SUPPLIER
        /// </summary>
        [HttpGet("products/{productId:int}")]
        public async Task<ActionResult<PagedResult<ProductResponse>>> GetProductById(int productId)
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

                var result = await _supplierService.GetProductByIdAsync(id, productId);

                if (result == null)
                    return Ok(HTTPResponse<object>.Response(400, "Not Found", result));

                return Ok(HTTPResponse<object>.Response(200, "Lấy sản phẩm thành công", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, HTTPResponse<object>.Response(500, "Internal server error", ex.Message));
            }
        }

        /// <summary>
        /// List order for supplier
        /// </summary>
        [HttpGet("orders")]
        public async Task<ActionResult<PagedResult<OrderResponse>>> GetListOrders(int pageNumber = 1, int pageSize = 10)
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

                var result = await _supplierService.GetPagedOrdersAsync(id, pageNumber, pageSize);

                if (result == null)
                    return Ok(HTTPResponse<object>.Response(400, "Not Found", result));

                return Ok(HTTPResponse<object>.Response(200, "Lấy danh sách đơn hàng thành công", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, HTTPResponse<object>.Response(500, "Internal server error", ex.Message));
            }
        }

        /// <summary>
        /// Get order for supplier
        /// </summary>
        [HttpGet("order/{orderId:Guid}")]
        public async Task<ActionResult<OrderResponse>> GetListOrders(Guid orderId)
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

                var result = await _supplierService.GetOrderByIdAsync(id, orderId);

                if (result == null)
                    return Ok(HTTPResponse<object>.Response(400, "Not Found", result));

                return Ok(HTTPResponse<object>.Response(200, "Lấy đơn hàng thành công", result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, HTTPResponse<object>.Response(500, "Internal server error", ex.Message));
            }
        }
        
        
        /// <summary>
        /// Đăng ký tài khoản mới
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterSupplierRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(HTTPResponse<object>.Response(400, "Dữ liệu không hợp lệ", null));
            }

            var (success, message) = await _authService.RegisterSupplierAsync(request.Phone, request.Password, request.Email, request.Fullname, request);

            if (!success)
            {
                return BadRequest(HTTPResponse<object>.Response(400, message.ToString(), null));
            }

            return Ok(HTTPResponse<object>.Response(200, message.ToString(), new
            {
                Username = request.Phone,
                Email = request.Email
            }));
        }
        
        
        [HttpGet("suppliers")]
        public async Task<IActionResult> GetAllSuppliers()
        {
            try
            {
                var suppliers = await _supplierService.GetAllSuppliersAsync();

                return Ok(HTTPResponse<object>.Response(200, "Fetched suppliers successfully", suppliers));
            }
            catch (Exception ex)
            {
                return StatusCode(500, HTTPResponse<object>.Response(500, "Internal server error", ex.Message));
            }
        }
        
        [HttpPut("update-permission/{supplierId}")]
        public async Task<IActionResult> UpdatePermission(int supplierId, [FromQuery] bool approve)
        {
            try
            {
                var result = await _supplierService.UpdatePermissionsAsync(supplierId, approve);

                // Nếu cập nhật thành công
                if (result == true)
                {
                    return Ok(HTTPResponse<object>.Response(200, "Permission updated successfully.", null));
                }
                // Nếu thất bại trong việc cập nhật quyền
                else if (result == false)
                {
                    return BadRequest(HTTPResponse<object>.Response(400, "Failed to update permission.", null));
                }
                // Nếu không tìm thấy nhà cung cấp
                else
                {
                    return BadRequest(HTTPResponse<object>.Response(400, "Supplier not found.", null));
                }
            }
            catch (Exception ex)
            {
                // Trả về lỗi khi có sự cố
                return StatusCode(500, HTTPResponse<object>.Response(500, "Internal server error", ex.Message));
            }
        }


    }
}
