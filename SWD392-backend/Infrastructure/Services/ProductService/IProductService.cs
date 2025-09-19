using cybersoft_final_project.Models.Request;
using SWD392_backend.Entities;
using SWD392_backend.Infrastructure.Repositories.ProductRepository;
using SWD392_backend.Models;
using SWD392_backend.Models.Request;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Services.ProductService
{
    public interface IProductService
    {
        Task<PagedResult<ProductResponse>> GetPagedProductAsync(int page, int pageSize);
        Task<ProductDetailResponse> GetByIdAsync(int id);
        Task<product> GetByIdEntityAsync(int id);
        Task<ProductDetailResponse> GetBySlugAsync(string slug);
        Task<ProductResponse> AddProductAsync(int id, AddProductRequest product);
        Task<ProductResponse> UpdateProductAsync(int id, int productId, UpdateProductRequest request);
        Task<bool> UpdateProductStatusAsync(int id, UpdateStatusProductRequest request);
        Task<bool> RemoveProductStatusAsync(int supplierId, int productId);
    }
}
