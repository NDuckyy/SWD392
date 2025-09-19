using SWD392_backend.Entities;

namespace SWD392_backend.Infrastructure.Repositories.ShipperRepository
{
    public interface IShipperRepository
    {
        Task<bool> AssignAreaAsync(shipper shipper);
        Task<shipper> GetShipperByUserIdAsync(int userId);

        Task<order> GetOrderByIdAsync(Guid orderId);
        Task<List<shipper>> GetAllShipper(string areaCode);
    }
}
