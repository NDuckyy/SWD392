using SWD392_backend.Context;

namespace SWD392_backend.Infrastructure.Repositories.OrderRepository;

// OrderRepository.cs
using SWD392_backend.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using SWD392_backend.Models;
using SWD392_backend.Entities.Enums;
using System.Collections.Generic;

public class OrderRepository : IOrderRepository
{
    private readonly MyDbContext _context;

    public OrderRepository(MyDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(order entity)
    {
        await _context.orders.AddAsync(entity);
    }
    
    
    public IQueryable<order> GetAll()
    {
        return _context.orders
            .Include(o => o.orders_details)
            .AsQueryable(); // Quan trọng để chuỗi LINQ hoạt động
    }

    public async Task<PagedResult<order>> GetOrdersToShipperAsync(string areaCode, int shipperId, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 10 : pageSize;

        //Total items
        var totalItems = await _context.orders
                        .Where(o => o.AreaCode == areaCode && o.ShipperId == shipperId)
                        .CountAsync();

        var orders = await _context.orders
                    .Include(o => o.user)
                    .Include(o => o.supplier)
                    .Include(o => o.orders_details)
                        .ThenInclude(od => od.product)
                            .ThenInclude(od => od.product_images)
                    .Where(o => o.AreaCode == areaCode && o.ShipperId == shipperId)
                    .Where(o => o.orders_details.Any(od => od.Status == OrderStatus.Preparing || od.Status == OrderStatus.Delivery || od.Status == OrderStatus.Delivered))
                    .OrderByDescending(o => o.CreatedAt)
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

    public async Task<List<orders_detail>> GetOrdersDetail(string orderId)
    {
        return await  _context.orders_details.Where(od => od.OrderId.ToString() == orderId).ToListAsync();
    }

    public async Task<int> GetTotalOrdersAsync()
    {
        return await _context.orders.CountAsync();
    }

    public async Task<int> CountOrdersByMonthAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.orders
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate)
                    .CountAsync();
    }

    public async Task<int> CountOrdersByDayAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.orders
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate)
                    .CountAsync();
    }

    public async Task<int> CountOrdersInRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.orders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .CountAsync();
    }

    public async Task<int> CountNewUsersInRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.users
            .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
            .CountAsync();
    }

    public async Task<PagedResult<order>> GetOrdersByMonthAsync(DateTime startDate, DateTime endDate, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 10 : pageSize;

        var totalItems = await CountOrdersByMonthAsync(startDate, endDate);

        var orders = await _context.orders
                    .Include(o => o.user)
                    .Include(o => o.supplier)
                    .Include(o => o.orders_details)
                        .ThenInclude(od => od.product)
                            .ThenInclude(od => od.product_images)
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate)
                    .OrderByDescending(o => o.CreatedAt)
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

    public async Task<PagedResult<order>> GetOrdersByDayAsync(DateTime startDate, DateTime endDate, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 10 : pageSize;

        var totalItems = await CountOrdersByDayAsync(startDate, endDate);

        var orders = await _context.orders
                    .Include(o => o.user)
                    .Include(o => o.supplier)
                    .Include(o => o.orders_details)
                        .ThenInclude(od => od.product)
                            .ThenInclude(od => od.product_images)
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                    .OrderByDescending(o => o.CreatedAt)
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

    public async Task<order?> GetOrderByIdAsync(Guid orderId)
    {
        return await _context.orders
                    .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public void Update(order order)
    {
        _context.orders.Update(order);
    }
    // Các method khác nếu cần
}