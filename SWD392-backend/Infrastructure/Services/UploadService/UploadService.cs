using System.Threading.Tasks;
using SWD392_backend.Entities;
using SWD392_backend.Infrastructure.Services.CategoryService;
using SWD392_backend.Infrastructure.Services.ElasticSearchService;
using SWD392_backend.Infrastructure.Services.ProductImageService;
using SWD392_backend.Infrastructure.Services.ProductService;
using SWD392_backend.Infrastructure.Services.S3Service;
using SWD392_backend.Infrastructure.Services.SupplerSerivce;
using SWD392_backend.Models.Request;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Services.UploadService
{
    public class UploadService : IUploadService
    {
        private readonly IS3Service _s3Service;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IProductImageService _productImageService;
        private readonly ISupplierService _supplierService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IElasticSearchService _elasticSearchService;

        public UploadService(IUnitOfWork unitOfWork, IS3Service s3Service, ICategoryService categoryService, IProductService productService, IProductImageService productImageService, IElasticSearchService elasticSearchService, ISupplierService supplierService)
        {
            _unitOfWork = unitOfWork;
            _s3Service = s3Service;
            _categoryService = categoryService;
            _productService = productService;
            _productImageService = productImageService;
            _elasticSearchService = elasticSearchService;
            _supplierService = supplierService;
        }

        public async Task<bool> ConfirmUploadImage(int id, List<string> imageUrl)
        {
            var product = await _productService.GetByIdEntityAsync(id);

            if (product == null)
                return false;

            await _productImageService.DeleteProductImagesByProductIdAsync(id);

            int index = 0;
            foreach (var url in imageUrl)
            {
                var image = new product_image
                {
                    ProductImageUrl = url,
                    ProductsId = id,
                    IsMain = (index == 0)
                };

                await _productImageService.AddProductImageFromUploadAsync(image);
                index++;
            }

            // Save
            await _unitOfWork.SaveAsync();

            product = await _productService.GetByIdEntityAsync(id);

            // Update to els
            await _elasticSearchService.UpdateProductAsync(product);

            return true;
        }

        public async Task<bool> ConfirmUploadSupplierImage(int id, List<string> imageUrl)
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(id);

            if (supplier == null)
                return false;

            var check = await _supplierService.AddIdCardImagesAsync(id, imageUrl);

            return check;
        }

        public async Task<UploadMultipleProductImgsResponse> UploadMultipleImage(UploadProductImgsRequest request, bool isSupplierId)
        {
            var uploads = new List<UploadProductImgResponse>();
            var cdnDomain = Environment.GetEnvironmentVariable("CDN_DOMAIN");

            for (int i = 0; i < request.ContentTypes.Count; i++)
            {
                var contentType = request.ContentTypes[i].Trim();
                var extension = contentType switch
                {
                    "image/jpeg" => "jpg",
                    "image/png" => "png",
                    "image/gif" => "gif",
                    "image/webp" => "webp",
                    "image/bmp" => "bmp",
                    "image/svg+xml" => "svg",
                    "image/avif" => "avif",
                    _ => "img"
                };

                string key;

                if (!isSupplierId)
                {
                    var categorySlug = await _categoryService.GetCategorySlugByIdAsync(request.CategoryId);
                    key = $"{categorySlug}/{request.ProductSlug}-{Guid.NewGuid()}-{request.ProductId}_{request.SupplierId}_{i + 1}.{extension}";
                }
                else
                {
                    key = $"suppliers/idcard-{Guid.NewGuid()}.{extension}";
                }

                var url = _s3Service.GeneratePreSignedURL(key, contentType);

                // Generate image link
                var imageUrl = $"{cdnDomain}/{key}";

                //Add to List
                uploads.Add(new UploadProductImgResponse
                {
                    Url = url,
                    Key = key,
                    ImageUrl = imageUrl
                });
            }

            return new UploadMultipleProductImgsResponse { Uploads = uploads };
        }
    }
}
