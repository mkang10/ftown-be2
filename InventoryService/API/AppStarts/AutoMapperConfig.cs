using Application.DTO.Request;
using Application.DTO.Response;
using AutoMapper;
using Domain.Entities;

namespace API.AppStarts
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            // Mapping từ Product -> ProductListResponse (Danh sách sản phẩm)
            CreateMap<Product, ProductListResponse>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Uncategorized"))
                .ForMember(dest => dest.MinPrice, opt => opt.MapFrom(src => src.ProductVariants.Any() ? src.ProductVariants.Min(v => v.Price) : 0))
                .ForMember(dest => dest.MaxPrice, opt => opt.MapFrom(src => src.ProductVariants.Any() ? src.ProductVariants.Max(v => v.Price) : 0))
                .ForMember(dest => dest.Colors, opt => opt.MapFrom(src => src.ProductVariants.Select(v => v.Color).Distinct().ToList()));

            // Mapping từ Product -> ProductDetailResponse (Chi tiết sản phẩm)
            CreateMap<Product, ProductDetailResponse>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Uncategorized"))
                .ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src.ProductVariants));

            // Mapping từ ProductVariant -> ProductVariantResponse
            CreateMap<ProductVariant, ProductVariantResponse>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));
            CreateMap<Store, StoreResponse>();

            CreateMap<StoreRequest, Store>()
                .ForMember(dest => dest.StoreId, opt => opt.Ignore()) // Ignore vì StoreId do DB sinh
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());
        }
    }
}