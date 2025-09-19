using Microsoft.IdentityModel.Tokens;
using SWD392_backend.Context;
using SWD392_backend.Entities;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using web_api_base.Helper; // sửa theo namespace dự án bạn
using Microsoft.Extensions.Configuration;
using SWD392_backend.Models.Request;

namespace SWD392_backend.Infrastructure.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(MyDbContext context, IConfiguration config, IUnitOfWork unitOfWork)
        {
            _context = context;
            _config = config;
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool Success, string Message, string? Token)> LoginAsync(string emailOrPhone,
            string password)
        {
            var user = _context.users.FirstOrDefault(u => u.Phone == emailOrPhone || u.Email == emailOrPhone);
            if (user == null || !PasswordHelper.VerifyPassword(password, user.Password))
            {
                return (false, "Sai số điện thoại hoặc mật khẩu", null);
            }

            var keyString = _config["Jwt:Key"] ?? throw new Exception("JWT Key is missing in configuration.");
            if (keyString.Length < 32)
            {
                throw new Exception("JWT key must be at least 32 characters (256 bits) for HMAC-SHA256.");
            }

            var key = Encoding.UTF8.GetBytes(keyString);
            var tokenHandler = new JwtSecurityTokenHandler();

            // 👉 Khởi tạo danh sách claim cơ bản
            var claims = new List<Claim>
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim("FullName", user.FullName ?? ""),
                new Claim("Role", user.Role ?? "")
            };

            // 👉 Nếu là SUPPLIER thì truy vấn bảng supplier và thêm SupplierId
            if (user.Role == "SUPPLIER")
            {
                var supplier = await _context.suppliers.FirstOrDefaultAsync(s => s.UserId == user.Id);
                if (supplier != null)
                {
                    claims.Add(new Claim("SupplierId", supplier.Id.ToString()));
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMonths(5),
                SigningCredentials =
                    new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return (true, "Đăng nhập thành công", jwt);
        }


        public async Task<(bool Success, string Message)> RegisterAsync(string phone, string password, string email,
            string fullname)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(phone))
                return (false, "Số điện thoại không được để trống.");

            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email không được để trống.");

            if (string.IsNullOrWhiteSpace(fullname))
                return (false, "Họ và tên không được để trống.");

            if (string.IsNullOrWhiteSpace(password))
                return (false, "Mật khẩu không được để trống.");

            // Kiểm tra tồn tại phone/email
            if (_context.users.Any(u => u.Phone.ToLower() == phone.ToLower()))
                return (false, "Số điện thoại đã tồn tại.");

            if (_context.users.Any(u => u.Email.ToLower() == email.ToLower()))
                return (false, "Email đã được sử dụng.");

            // Tạo username từ fullname: loại bỏ khoảng trắng, viết thường
            var username = new string(fullname.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToLower();

            var hashedPassword = PasswordHelper.HashPassword(password);

            var newUser = new user
            {
                Username = username,
                Password = hashedPassword,
                Email = email,
                Address = string.Empty,
                Role = "CUSTOMER",
                FullName = fullname,
                Phone = phone,
                ImageUrl = "http://default-avatar.com/avatar.png",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.UserRepository.AddAsync(newUser);
            await _unitOfWork.SaveAsync();

            return (true, "Đăng ký thành công");
        }

        public async Task<(bool success, object message)> RegisterSupplierAsync(string requestPhone,
            string requestPassword, string requestEmail, string requestFullname,
            RegisterSupplierRequest registerSupplierRequest)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(requestPhone))
                return (false, "Số điện thoại không được để trống.");

            if (string.IsNullOrWhiteSpace(requestEmail))
                return (false, "Email không được để trống.");

            if (string.IsNullOrWhiteSpace(requestFullname))
                return (false, "Họ và tên không được để trống.");

            if (string.IsNullOrWhiteSpace(requestPassword))
                return (false, "Mật khẩu không được để trống.");

            // Kiểm tra tồn tại phone/email
            if (_context.users.Any(u => u.Phone.ToLower() == requestPhone.ToLower()))
                return (false, "Số điện thoại đã tồn tại.");

            if (_context.users.Any(u => u.Email.ToLower() == requestEmail.ToLower()))
                return (false, "Email đã được sử dụng.");

            // Tạo username từ fullname: loại bỏ khoảng trắng, viết thường
            var username = new string(requestFullname.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToLower();

            // Mã hóa mật khẩu
            var hashedPassword = PasswordHelper.HashPassword(requestPassword);

            // Tạo user
            var newUser = new user
            {
                Username = username,
                Password = hashedPassword,
                Email = requestEmail,
                Address = string.Empty,
                Role = "CUSTOMER",
                FullName = requestFullname,
                Phone = requestPhone,
                ImageUrl = "http://default-avatar.com/avatar.png",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Start transaction
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Add the user and save changes
                    await _unitOfWork.UserRepository.AddAsync(newUser);
                    await _unitOfWork.SaveAsync(); // Save the user first to generate the UserId

                    // Create supplier and associate it with the user
                    var supplier = new supplier
                    {
                        UserId = newUser.Id, // Ensure this is the correct UserId
                        Name = requestFullname,
                        Slug = requestFullname.ToLower().Replace(" ", "-"),
                        RegisteredAt = DateTime.UtcNow,
                        IsVerified = false,
                        Description = "description",
                        ImageUrl = "http://default-avatar.com/avatar.png",
                        FrontImageCCCD = registerSupplierRequest.front_image, // Assuming it's part of the request
                        BackImageCCCD = registerSupplierRequest.back_image // Assuming it's part of the request
                    };

                    // Add supplier and save changes
                    await _unitOfWork.SupplierRepository.AddAsync(supplier);
                    await _unitOfWork.SaveAsync();

                    // Commit the transaction
                    await transaction.CommitAsync();

                    return (true, "Đăng ký thành công");
                }
                catch (Exception ex)
                {
                    // Rollback the transaction if something fails
                    await transaction.RollbackAsync();
                    return (false, $"Đã có lỗi xảy ra: {ex.Message}");
                }
            }
        }
    }
}