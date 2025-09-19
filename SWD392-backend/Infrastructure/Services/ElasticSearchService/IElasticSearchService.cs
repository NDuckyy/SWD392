using SWD392_backend.Entities;
using SWD392_backend.Models;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Services.ElasticSearchService
{
    public interface IElasticSearchService
    {
        Task<PagedResult<ProductResponse>> SearchAsync(
            string q = "",
            int? categoryId = null,
            int page = 1,
            int size = 10,
            string sortBy = "createdAt",
            string sortOrder = "desc"
        );

        Task IndexProductAsync(product product);
        Task UpdateProductAsync(product product);
        Task RemoveProductAsync(int id);
    }
}
