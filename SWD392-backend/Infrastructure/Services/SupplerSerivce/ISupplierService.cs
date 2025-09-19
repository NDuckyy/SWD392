using SWD392_backend.Entities;
using SWD392_backend.Models;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Services.SupplerSerivce;

public interface ISupplierService
{   
    Task<int> GetTotalSuppliersAsync();
    Task<SupplierProfileResponse> GetSupplierByIdAsync(int id);

    Task<PagedResult<ProductResponse>> GetPagedProductsAsync(int supplierId, int pageNumber, int pageSize);
    Task<ProductDetailResponse> GetProductByIdAsync(int id, int productId);
    Task<product> GetProductToRemoveAsync(int id, int productId);
    Task<PagedResult<OrderResponse>> GetPagedOrdersAsync(int supplierId, int pageNumber, int pageSize);
    Task<OrderResponse> GetOrderByIdAsync(int id, Guid orderId);
    Task<bool> AddIdCardImagesAsync(int id, List<string> imageUrl);

        Task<List<SupplierResponse>> GetAllSuppliersAsync();


        Task<bool?> UpdatePermissionsAsync(int supplierI,  bool approve);
}