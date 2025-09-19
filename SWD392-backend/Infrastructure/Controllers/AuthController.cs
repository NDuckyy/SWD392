using cybersoft_final_project.Models;
using cybersoft_final_project.Models.Request;
using Microsoft.AspNetCore.Mvc;
using SWD392_backend.Infrastructure.Services.AuthService;

namespace SWD392_backend.Infrastructure.Controllers;



[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService  _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Đăng nhập và trả về JWT nếu thành công
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(HTTPResponse<object>.Response(400, "Dữ liệu không hợp lệ", null));
        }

        var result = await _authService.LoginAsync(request.EmailOrPhone, request.Password);

        if (!result.Success)
        {
            return Unauthorized(HTTPResponse<object>.Response(401, result.Message, null));
        }

        return Ok(HTTPResponse<object>.Response(200, result.Message, new
        {
            Token = result.Token
        }));
    }

    /// <summary>
    /// Đăng xuất - chỉ hướng dẫn xóa token phía client
    /// </summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(HTTPResponse<object>.Response(200, " Đăng xuất thành công. Hãy xóa token ở phía client.", null));
    }

    /// <summary>
    /// Đăng ký tài khoản mới
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(HTTPResponse<object>.Response(400, "Dữ liệu không hợp lệ", null));
        }

        var (success, message) = await _authService.RegisterAsync(request.Phone, request.Password, request.Email, request.Fullname);

        if (!success)
        {
            return BadRequest(HTTPResponse<object>.Response(400, message, null));
        }

        return Ok(HTTPResponse<object>.Response(200, message, new
        {
            Username = request.Phone,
            Email = request.Email
        }));
    }
}
