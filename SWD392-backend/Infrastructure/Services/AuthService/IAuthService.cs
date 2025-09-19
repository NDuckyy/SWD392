using System.Threading.Tasks;
using SWD392_backend.Models.Request;

namespace SWD392_backend.Infrastructure.Services.AuthService
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, string? Token)> LoginAsync(string emailOrPhone, string password);
        Task<(bool Success, string Message)> RegisterAsync(string username, string password, string email, string fullname);
        Task<(bool success, object message)> RegisterSupplierAsync(string requestPhone, string requestPassword, string requestEmail, string requestFullname, RegisterSupplierRequest registerSupplierRequest);
    }
}