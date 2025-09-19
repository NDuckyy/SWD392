using SWD392_backend.Entities;
using SWD392_backend.Models.Request;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Services.ShipperService
{
    public interface IShipperService
    {
        Task<bool> AssignAreaAsync(int userId, AssignAreaRequest request);
        Task<List<shipper>> GetAllShippers(string areaCode);
        Task<OrderResponse> GetOrderByIdAsync(int id, Guid orderId);
        Task<ShipperProfileResponse> GetShipperByUserIdAsync(int userId);
    }
}
