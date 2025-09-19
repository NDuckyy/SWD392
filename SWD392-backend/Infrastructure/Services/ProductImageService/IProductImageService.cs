using SWD392_backend.Entities;
using SWD392_backend.Models.Request;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Services.ProductImageService
{
    public interface IProductImageService
    {
        Task<List<ProductImageResponse>> AddProductImageAsync(int productId, List<ProductImageRequest> request);
        Task<product_image> AddProductImageFromUploadAsync(product_image productImage);
        Task<product_image> GetProductImageByProductIdAsync(int productId);
        Task<List<string>> GetAllImages(int productId);
        Task DeleteProductImagesByProductIdAsync(int id);
    }
}
