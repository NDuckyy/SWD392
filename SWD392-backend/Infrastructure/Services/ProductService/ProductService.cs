using System.Threading.Tasks;
using AutoMapper;
using cybersoft_final_project.Models.Request;
using SWD392_backend.Entities;
using SWD392_backend.Infrastructure.Repositories.ProductRepository;
using SWD392_backend.Infrastructure.Services.ElasticSearchService;
using SWD392_backend.Infrastructure.Services.ProductImageService;
using SWD392_backend.Infrastructure.Services.S3Service;
using SWD392_backend.Infrastructure.Services.SupplerSerivce;
using SWD392_backend.Models;
using SWD392_backend.Models.Request;
using SWD392_backend.Models.Response;
using SWD392_backend.Utilities;

namespace SWD392_backend.Infrastructure.Services.ProductService
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IElasticSearchService _elasticSearchService;
        private readonly IS3Service _s3Service;
        private readonly IProductImageService _productImageService;
        private readonly ISupplierService _supplierService;

        public ProductService(IProductRepository productRepository, IMapper mapper, IUnitOfWork unitOfWork, IElasticSearchService elasticSearchService, IS3Service s3Service, IProductImageService productImageService, ISupplierService supplierService)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _elasticSearchService = elasticSearchService;
            _s3Service = s3Service;
            _productImageService = productImageService;
            _supplierService = supplierService;
        }

        public async Task<ProductDetailResponse> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            // Model mapper
            var response = _mapper.Map<ProductDetailResponse>(product);

            return response;
        }


        public async Task<product> GetByIdEntityAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product;
        }

        public async Task<ProductDetailResponse> GetBySlugAsync(string slug)
        {
            var product = await _productRepository.GetBySlugAsync(slug);

            // Model mapper
            var response = _mapper.Map<ProductDetailResponse>(product);

            return response;
        }

        public async Task<PagedResult<ProductResponse>> GetPagedProductAsync(int page, int pageSize)
        {
            var pagedResult = await _productRepository.GetPagedProductsAsync(page, pageSize);

            // Model mapper
            var productDtos = _mapper.Map<List<ProductResponse>>(pagedResult.Items);


            return new PagedResult<ProductResponse>
            {
                Items = productDtos,
                TotalItems = pagedResult.TotalItems,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<ProductResponse> AddProductAsync(int id, AddProductRequest request)
        {

            // Map from request
            var product = _mapper.Map<product>(request);

            // Add another field
            product.CreatedAt = DateTime.UtcNow;
            product.DiscountPrice = product.Price - (product.Price * product.DiscountPercent / 100);
            product.AvailableQuantity = product.StockInQuantity - product.SoldQuantity;
            product.IsActive = true;
            product.Slug = SlugHelper.Slugify(product.Name);
            product.SupplierId = id;

            // Insert
            await _productRepository.AddAsync(product);

            // Save
            await _unitOfWork.SaveAsync();

            // Index into Elastic Search
            await _elasticSearchService.IndexProductAsync(product);

            var response = _mapper.Map<ProductResponse>(product);
            return response;
        }

        public async Task<ProductResponse> UpdateProductAsync(int id, int productId, UpdateProductRequest request)
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(id);

            if (supplier == null)
                return null;

            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
                return null;

            if (supplier.Id != product.SupplierId)
                return null;

            // Map into exist product
            _mapper.Map(request, product);

            product.DiscountPrice = product.Price - (product.Price * product.DiscountPercent / 100);
            product.AvailableQuantity = product.StockInQuantity - product.SoldQuantity;
            product.IsActive = true;
            product.Slug = SlugHelper.Slugify(product.Name);

            // Update
            _unitOfWork.ProductRepository.Update(product);

            // Save
            await _unitOfWork.SaveAsync();

            // Update into Elastic Search
            await _elasticSearchService.UpdateProductAsync(product);

            var response = _mapper.Map<ProductResponse>(product);

            return response;
        }

        public async Task<bool> UpdateProductStatusAsync(int id, UpdateStatusProductRequest request)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                return false;

            product.IsActive = request.IsActive;

            // Save
            await _unitOfWork.SaveAsync();

            // Update into Elastic Search
            await _elasticSearchService.UpdateProductAsync(product);

            return true;
        }

        public async Task<bool> RemoveProductStatusAsync(int supplierId, int productId)
        {
            var product = await _supplierService.GetProductToRemoveAsync(supplierId, productId);
            if (product == null)
                return false;

            var listUrls = await _productImageService.GetAllImages(productId);

            // Remove image
            await _productRepository.RemoveImagesByProductIdAsync(productId);

            // Remove image from s3
            if (listUrls.Count > 0)
                await _s3Service.DeleteFileAsync(listUrls);

            // Remove product
            await _productRepository.RemoveAsync(product);

            // Save to DB
            await _unitOfWork.SaveAsync();

            // Remove from els
            await _elasticSearchService.RemoveProductAsync(productId);
    
            return true;
        }
    }
}
