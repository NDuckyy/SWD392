using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SWD392_backend.Context;
using SWD392_backend.Entities;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Repositories.CategoryRepository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly MyDbContext _context;

        public CategoryRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<List<category>> GetCategoriesAsync()
        {
            return await _context.categories.ToListAsync();
        }

        public async Task<string> GetCategorySlugByIdAsync(int id)
        {
            return await _context.categories
                        .Where(c => c.Id == id)
                        .Select(c => c.Slug)
                        .FirstOrDefaultAsync();
        }
    }
}
