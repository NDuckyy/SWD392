using SWD392_backend.Entities;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Services.CategoryService
{
    public interface ICategoryService
    {
        Task<List<CategoryResponse>> GetCategoriesAsync();
        Task<string> GetCategorySlugByIdAsync(int id);
    }
}
