using AutoMapper;
using Elastic.Clients.Elasticsearch.Requests;
using SWD392_backend.Entities;
using SWD392_backend.Infrastructure.Repositories.ProductImageRepository;
using SWD392_backend.Models.Request;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Services.ProductImageService
{
    public class ProductImageService : IProductImageService
    {
        private readonly IProductImageRepository _productImageRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductImageService(IUnitOfWork unitOfWork, IMapper mapper, IProductImageRepository productImageRepository)
        {
            _productImageRepository = productImageRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ProductImageResponse>> AddProductImageAsync(int productId, List<ProductImageRequest> request)
        {
            // Remove main check
            var currentMainImages = await _productImageRepository.FindAllMainImage(productId);

            foreach (var img in currentMainImages)
            {
                img.IsMain = false;
            }

            var images = _mapper.Map<List<product_image>>(request);

            foreach (var img in images)
            {
                img.ProductsId = productId;
            }

            await _productImageRepository.AddImages(images);
            await _unitOfWork.SaveAsync();

            var response = _mapper.Map<List<ProductImageResponse>>(images);

            return response;
        }

        public async Task<product_image> AddProductImageFromUploadAsync(product_image productImage)
        {

            await _productImageRepository.AddImage(productImage);
            await _unitOfWork.SaveAsync();

            return productImage;
        }

        public async Task DeleteProductImagesByProductIdAsync(int id)
        {
            await _productImageRepository.DeleteProductImagesByProductIdAsync(id);
        }

        public async Task<List<string>> GetAllImages(int productId)
        {
            var listImageUrls = await _productImageRepository.GetAllImagesAsync(productId);

            if (listImageUrls.Count < 0)
                return null;
            else 
                return listImageUrls;
        }

        public async Task<product_image> GetProductImageByProductIdAsync(int productId)
        {
            var productImage = await _productImageRepository.GetProductImageByProductIdAsync(productId);

            return productImage;
        }
    }
}
