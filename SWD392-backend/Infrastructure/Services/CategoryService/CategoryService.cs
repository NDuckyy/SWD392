using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SWD392_backend.Entities;
using SWD392_backend.Infrastructure.Repositories.CategoryRepository;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Services.CategoryService
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<List<CategoryResponse>> GetCategoriesAsync()
        {
            var categories = await _categoryRepository.GetCategoriesAsync();

            var categoryDtos = _mapper.Map<List<CategoryResponse>>(categories);

            return categoryDtos;
        }

        public async Task<string> GetCategorySlugByIdAsync(int id)
        {
            return await _categoryRepository.GetCategorySlugByIdAsync(id);
        }
    }
}
