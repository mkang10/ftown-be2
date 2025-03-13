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
            //Exxcel
            CreateMap<Product, ProductExcelRequestDTO>().ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Category.Name));
            CreateMap<Product, CreateProductDTORequest>().ReverseMap();


            CreateMap<Product, ProductListResponse>()
                .ForMember(dest => dest.CategoryName,
                           opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Uncategorized"))
                .ForMember(dest => dest.Price,
                   opt => opt.MapFrom(src => src.ProductVariants.Any()
                       ? src.ProductVariants.First().Price
                       : 0))
                .ForMember(dest => dest.Colors,
                           opt => opt.MapFrom(src => src.ProductVariants.Select(v => v.Color).Distinct().ToList()))
                .ForMember(dest => dest.ImagePath,
               opt => opt.MapFrom(src => src.ProductImages
                                          .Where(pi => pi.IsMain)
                                          .Select(pi => pi.ImagePath)
                                          .FirstOrDefault()));

            // Mapping từ Product -> ProductDetailResponse (Chi tiết sản phẩm)
            CreateMap<Product, ProductDetailResponse>()
                 .ForMember(dest => dest.CategoryName,
                            opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Uncategorized"))
                 .ForMember(dest => dest.Variants,
                            opt => opt.MapFrom(src => src.ProductVariants))
                 .ForMember(dest => dest.ImagePath, // Ảnh chính
                            opt => opt.MapFrom(src => src.ProductImages
                                                       .Where(i => i.IsMain)
                                                       .Select(i => i.ImagePath)
                                                       .FirstOrDefault()))
                 .ForMember(dest => dest.ImagePaths, // Danh sách ảnh
                            opt => opt.MapFrom(src => src.ProductImages
                                                       .Select(i => i.ImagePath)
                                                       .ToList()));
            // Mapping từ ProductVariant -> ProductVariantResponse
            CreateMap<ProductVariant, ProductVariantResponse>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));
            CreateMap<Store, StoreResponse>();

            CreateMap<StoreRequest, Store>()
                .ForMember(dest => dest.StoreId, opt => opt.Ignore()) // Ignore vì StoreId do DB sinh
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());
            CreateMap<ProductVariantRequest, ProductVariant>().ReverseMap();
        }
    }
}