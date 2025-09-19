using SWD392_backend.Entities;

namespace SWD392_backend.Infrastructure.Repositories.CategoryRepository
{
    public interface ICategoryRepository
    {
        Task<List<category>> GetCategoriesAsync();
        Task<string> GetCategorySlugByIdAsync(int id);
    }
}
