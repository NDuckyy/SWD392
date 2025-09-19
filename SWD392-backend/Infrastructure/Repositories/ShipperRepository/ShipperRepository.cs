using Microsoft.EntityFrameworkCore;
using SWD392_backend.Context;
using SWD392_backend.Entities;

namespace SWD392_backend.Infrastructure.Repositories.ShipperRepository
{
    public class ShipperRepository : IShipperRepository
    {
        private readonly MyDbContext _context;

        public ShipperRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AssignAreaAsync(shipper shipper)
        {
            _context.shipper.Update(shipper);
            var affectedRows = await _context.SaveChangesAsync();
            return affectedRows > 0;
        }

        public async Task<List<shipper>> GetAllShipper(string areaCode)
        {
            return await _context.shipper.Where(s => s.AreaCode == areaCode).ToListAsync();
        }

        public async Task<order?> GetOrderByIdAsync(Guid orderId)
        {
            return await _context.orders
                        .Include(o => o.orders_details)
                            .ThenInclude(od => od.product)
                                .ThenInclude(od => od.product_images)
                        .Include(o => o.user)
                        .Include(o => o.supplier)
                        .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<shipper?> GetShipperByUserIdAsync(int userId)
        {
            return await _context.shipper
                        .Include(s => s.user)
                        .FirstOrDefaultAsync(s => s.UserId == userId);
        }
    }
}
