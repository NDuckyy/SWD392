using Elastic.Clients.Elasticsearch.Security;
using Microsoft.EntityFrameworkCore;
using SWD392_backend.Context;
using SWD392_backend.Entities;
using Microsoft.EntityFrameworkCore;
using SWD392_backend.Models.Response;
using SWD392_backend.Models;

namespace SWD392_backend.Infrastructure.Repositories.UserRepository
{
    public class UserRepository : IUserRepository
    {
        private readonly MyDbContext _context;

        public UserRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<user>> GetAllUserAsync(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : pageSize;

            // Total items
            var totalItems = await _context.users.CountAsync();

            var users = await _context.users
                .OrderByDescending(u => u.CreatedAt)
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<user>
            {
                Items = users,
                TotalItems = totalItems,
                Page = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<user?> GetUserByIdAsync(int id)
        {
            return await _context.users.FindAsync(id);
        }

        public async Task AddAsync(user entity)
        {
            await _context.users.AddAsync(entity);
        }

        public async Task<int> CountAsync()
        {
            return await _context.users.CountAsync();
        }

        public async Task<user?> GetUserByEmail(string requestEmail)
        {
            return await _context.users.Where(u => u.Email == requestEmail).FirstOrDefaultAsync();
        }

        public async Task<int> GetTotalUserByMonth(int month, int year)
        {
            var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);
            return await _context.users
                .Where(u => u.CreatedAt >= start && u.CreatedAt < end)
                .CountAsync();
        }

        public async Task<int> CountUsersBetween(DateTime start, DateTime end)
        {
            return await _context.users
                .Where(u => u.CreatedAt >= start && u.CreatedAt < end)
                .CountAsync();
        }

        public async Task<user> UpdateUserStatusAsync(int? userId)
        {
            var existingUser = await _context.users.FirstOrDefaultAsync(u => u.Id == userId);

            if (existingUser == null)
                return null;
            existingUser.IsActive = !existingUser.IsActive;
    
            _context.users.Update(existingUser);
            await _context.SaveChangesAsync();

            return existingUser;
        }
    }
}