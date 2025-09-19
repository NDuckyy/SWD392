using Microsoft.EntityFrameworkCore;
using SWD392_backend.Context;
using SWD392_backend.Entities;
using SWD392_backend.Models;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Repositories.SupplierRepository;

public class SupplierRepository : ISupplierRepository
{
    private readonly MyDbContext _context;

    public SupplierRepository(MyDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<product>> GetPagedProductsAsync(int supplierId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;

        // Total Items
        var totalItems = await _context.products.Where(i => i.SupplierId == supplierId).CountAsync();

        var products = await _context.products
                            .Include(p => p.product_attributes)
                            .Include(p => p.product_images)
                            .Include(p => p.categories)
                            .Where(p => p.SupplierId == supplierId)
                            .OrderByDescending(p => p.CreatedAt)
                            .AsNoTracking()
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();

        return new PagedResult<product>
        {
            Items = products,
            TotalItems = totalItems,
            Page = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<order>> GetPagedOrdersAsync(int supplierId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;

        // Total Items
        var totalItems = await _context.orders.Where(o => o.SupplierId == supplierId).CountAsync();

        var orders = await _context.orders
                            .Include(o => o.user)
                            .Include(o => o.supplier)
                            .Include(o => o.orders_details)
                                .ThenInclude(od => od.product)
                                    .ThenInclude(od => od.product_images)
                            .Where(o => o.SupplierId == supplierId)
                            .OrderByDescending(p => p.CreatedAt)
                            .AsNoTracking()
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();

        return new PagedResult<order>
        {
            Items = orders,
            TotalItems = totalItems,
            Page = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<product?> GetProductByIdAsync(int id, int productId)
    {
        return await _context.products
                                .Include(p => p.categories)
                                .Include(p => p.supplier)
                                .Include(p => p.product_images)
                                .FirstOrDefaultAsync(p => p.Id == productId && p.SupplierId == id);
    }

    public async Task<supplier?> GetSupplierByIdAsync(int id)
    {
        return await _context.suppliers
                                .Include(s => s.user)
                                .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.suppliers.CountAsync();
    }

    public async Task<order?> GetOrderByIdAsync(int id, Guid orderId)
    {
        return await _context.orders
                    .Include(o => o.user)
                    .Include(o => o.supplier)
                    .Include(o => o.orders_details)
                        .ThenInclude(od => od.product)
                            .ThenInclude(od => od.product_images)
                    .Where(o => o.SupplierId == id)
                    .FirstOrDefaultAsync(o => o.Id == orderId);
    }


    public async Task AddAsync(supplier supplier)
    {
        await _context.suppliers.AddAsync(supplier);
    }

    public async Task<bool> AddIdCardImagesAsync(int id, List<string> imageUrl)
    {
        var supplier = await GetSupplierByIdAsync(id);

        if (supplier == null)
            return false;

        supplier.FrontImageCCCD = imageUrl.Count > 0 ? imageUrl[0] : null;
        supplier.BackImageCCCD = imageUrl.Count > 1 ? imageUrl[1] : null;

        _context.SaveChanges();
        return true;
    }

    public async Task<bool> DeleteIdCardImagesById(int id)
    {
        var supplier = await GetSupplierByIdAsync(id);

        if (supplier == null)
            return false;

        supplier.FrontImageCCCD = null;
        supplier.BackImageCCCD = null;

       _context.SaveChanges();
       return true;
    }

    public async Task<bool> UpdatePermission(int supplierId, bool approve)
    {
        // Tìm supplier và bao gồm thông tin user
        var supplier = await _context.suppliers
            .Where(p => p.Id == supplierId)
            .Include(p => p.user) // Bao gồm thông tin user
            .FirstOrDefaultAsync();

        if (supplier == null)
        {
            return false; // Supplier không tồn tại
        }

        if (approve)
        {
            // Cập nhật trạng thái 'IsVerified' của supplier thành true
            supplier.IsVerified = true;
            // Không thay đổi role của user, chỉ cập nhật trạng thái verified
            supplier.user.Role = "SUPPLIER";
        }
        else
        {
            // Xóa user và supplier khỏi database
            var user = _context.users.FirstOrDefault(u => u.Id == supplier.user.Id);
            if (user != null)
            {
                _context.users.Remove(user);
            }
            _context.suppliers.Remove(supplier);
        }

        // Lưu thay đổi vào database
        await _context.SaveChangesAsync();

        return true; // Cập nhật thành công
    }



    public async Task<List<SupplierResponse>> GetAllSupplierAsync()
    {
        return  await _context.suppliers
            .Select(s => new SupplierResponse
            {
                Id = s.Id,
                Name = s.Name,
                Slug = s.Slug,
                Description = s.Description,
                ImageUrl = s.ImageUrl,
                IsVerified = s.IsVerified,
                FrontImageCCCD = s.FrontImageCCCD,
                BackImageCCCD = s.BackImageCCCD,
                RegisteredAt = s.RegisteredAt,
                UserId = s.user.Id
            })
            .ToListAsync();
    }
}

