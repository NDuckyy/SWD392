using AutoMapper;
using cybersoft_final_project.Models.Request;
using SWD392_backend.Entities;
using SWD392_backend.Models.ElasticDocs;
using SWD392_backend.Models.Request;
using SWD392_backend.Models.Response;

namespace SWD392_backend.Infrastructure.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<user, UserProfileResponse>();
            CreateMap<shipper, ShipperProfileResponse>()
                .ForMember(dest => dest.UserProfile, opt => opt.MapFrom(src => src.user));
            CreateMap<supplier, SupplierProfileResponse>()
                .ForMember(dest => dest.UserProfile, opt => opt.MapFrom(src => src.user))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Name));
            CreateMap<product, ProductResponse>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.product_images.Count(i => i.IsMain) > 0 ? src.product_images.FirstOrDefault(i => i.IsMain).ProductImageUrl : "https://placehold.co/150"))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(p => p.categories.Name));
            CreateMap<UpdateProductRequest, product>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
            CreateMap<product, ProductDetailResponse>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.product_images));
            CreateMap<AddProductRequest, product>();
            CreateMap<product, ProductElasticDoc>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.product_images.Count(i => i.IsMain) > 0 ? src.product_images.FirstOrDefault(i => i.IsMain).ProductImageUrl : "https://placehold.co/150"));
            CreateMap<category, CategoryResponse>();
            CreateMap<supplier, SupplierrResponse>();
            CreateMap<ProductImageRequest, product_image>();
            CreateMap<product_image, ProductImageResponse>();
            CreateMap<order, OrderResponse>()
                .ForMember(dest => dest.orders_details, opt => opt.MapFrom(src => src.orders_details))
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.user.FullName))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.supplier.Name));

            CreateMap<orders_detail, OrderDetailResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.product.Name))
                .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => src.product.product_images.FirstOrDefault(p => p.IsMain).ProductImageUrl));
    

            CreateMap<ReviewRequest, product_review>();
            CreateMap<product_review, ReviewResponse>();

            CreateMap<user, UserResponse>();
        }
    }
}