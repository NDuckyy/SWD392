using SWD392_backend.Models.Request;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Services.UploadService
{
    public interface IUploadService
    {
        Task<bool> ConfirmUploadImage(int id, List<string> imageUrl);
        Task<bool> ConfirmUploadSupplierImage(int id, List<string> imageUrl);
        Task<UploadMultipleProductImgsResponse> UploadMultipleImage(UploadProductImgsRequest request, bool isSupplierId);
    }
}
