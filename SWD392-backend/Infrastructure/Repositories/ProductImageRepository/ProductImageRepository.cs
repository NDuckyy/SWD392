using Microsoft.EntityFrameworkCore;
using SWD392_backend.Context;
using SWD392_backend.Entities;
using static System.Net.Mime.MediaTypeNames;

namespace SWD392_backend.Infrastructure.Repositories.ProductImageRepository
{
    public class ProductImageRepository : IProductImageRepository
    {
        private readonly MyDbContext _context;

        public ProductImageRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddImage(product_image image)
        {
            await _context.product_images.AddAsync(image);
        }

        public Task AddImages(List<product_image> images)
        {
            return _context.product_images.AddRangeAsync(images);
        }

        public async Task DeleteProductImagesByProductIdAsync(int id)
        {
            var images = _context.product_images.Where(x => x.ProductsId == id);
            _context.product_images.RemoveRange(images);
            await _context.SaveChangesAsync();
        }

        public async Task<List<product_image>> FindAllMainImage(int productId)
        {
            return await _context.product_images
                        .Where(img => img.ProductsId == productId && img.IsMain)
                        .ToListAsync();
        }

        public async Task<List<string>> GetAllImagesAsync(int productId)
        {
            return await _context.product_images
                        .Where(p => p.ProductsId == productId)
                        .Select(p => p.ProductImageUrl)
                        .ToListAsync();
        }

        public async Task<product_image?> GetProductImageByProductIdAsync(int productId)
        {
            return await _context.product_images
                        .Where(img => img.ProductsId == productId)
                        .FirstOrDefaultAsync();
        }
    }
}
