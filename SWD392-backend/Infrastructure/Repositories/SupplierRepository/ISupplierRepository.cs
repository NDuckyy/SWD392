using SWD392_backend.Entities;
using SWD392_backend.Models;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Repositories.SupplierRepository;

public interface ISupplierRepository
{
    Task<int> GetTotalCountAsync();
    Task<supplier> GetSupplierByIdAsync(int id);
    Task<PagedResult<product>> GetPagedProductsAsync(int supplierId, int pageNumber, int pageSize);
    Task<product> GetProductByIdAsync(int id, int productId);
    Task<PagedResult<order>> GetPagedOrdersAsync(int supplierId, int pageNumber, int pageSize);
    Task<order> GetOrderByIdAsync(int id, Guid orderId);
    Task AddAsync(supplier supplier);
   Task<List<SupplierResponse>> GetAllSupplierAsync();

    Task<bool> AddIdCardImagesAsync(int id, List<string> imageUrl);
    Task<bool> DeleteIdCardImagesById(int id);

    Task<bool> UpdatePermission(int supplierId, bool approve);
}