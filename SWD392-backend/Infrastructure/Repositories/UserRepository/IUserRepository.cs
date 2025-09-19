using SWD392_backend.Entities;
using SWD392_backend.Models;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Repositories.UserRepository
{
    public interface IUserRepository
    {
        Task<PagedResult<user>> GetAllUserAsync(int pageNumber, int pageSize);
        Task<user?> GetUserByIdAsync(int id);
        
        Task AddAsync(user entity);

        Task<int> CountAsync();


        Task<user?> GetUserByEmail(string requestEmail);
        Task<int> GetTotalUserByMonth(int month, int year);
        
        Task<int> CountUsersBetween(DateTime start, DateTime end);

        Task<user> UpdateUserStatusAsync(int? userId);

    }
}
