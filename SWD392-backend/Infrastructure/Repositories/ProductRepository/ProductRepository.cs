using Microsoft.EntityFrameworkCore;
using SWD392_backend.Context;
using SWD392_backend.Entities;
using SWD392_backend.Models;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Repositories.ProductRepository
{
    public class ProductRepository : IProductRepository
    {
        private readonly MyDbContext _context;

        public ProductRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(product product)
        {
            await _context.products.AddAsync(product);
        }

        public async Task RemoveAsync(product product)
        {
            _context.products.Remove(product);
            await Task.CompletedTask;        
        }

        public Task RemoveImagesByProductIdAsync(int productId)
        {
            var images = _context.product_images.Where(p => p.ProductsId == productId);
            _context.product_images.RemoveRange(images);
            return Task.CompletedTask;
        }

        public async Task<product?> GetByIdAsync(int id)
        {
            return await _context.products
                                .Include(p => p.categories)
                                .Include(p => p.supplier)
                                .Include(p => p.product_images)
                                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<product?> GetBySlugAsync(string slug)
        {
            return await _context.products
                                .Include(p => p.categories)
                                .Include(p => p.supplier)
                                .Include(p => p.product_images)
                                .FirstOrDefaultAsync(p => p.Slug.ToLower() == slug.ToLower());
        }

        public async Task<PagedResult<product>> GetPagedProductsAsync(int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;

            // Total items
            var totalItems = await _context.products.CountAsync();

            var products = await _context.products
                            .Include(p => p.product_attributes)
                            .Include(p => p.product_images)
                            .Include(p => p.categories)
                            .Where(p => p.IsActive)
                            .OrderByDescending(p => p.CreatedAt)
                            .AsNoTracking() 
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();

            return new PagedResult<product>
            {
                Items = products,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }

        public void Update(product product)
        {
            _context.products.Update(product);
        }
    }
}
