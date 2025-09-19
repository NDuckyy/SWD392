using SWD392_backend.Entities;

namespace SWD392_backend.Infrastructure.Repositories.ProductImageRepository
{
    public interface IProductImageRepository
    {
        Task AddImages(List<product_image> images);
        Task AddImage(product_image image);
        Task<List<product_image>> FindAllMainImage(int productId);
        Task<product_image> GetProductImageByProductIdAsync(int productId);
        Task<List<string>> GetAllImagesAsync(int productId);
        Task DeleteProductImagesByProductIdAsync(int id);
    }
}
