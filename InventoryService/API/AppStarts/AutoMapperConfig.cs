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
            CreateMap<Product, ProductListResponse>()
                .ForMember(dest => dest.CategoryName,
                           opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Uncategorized"))
                .ForMember(dest => dest.Price,
                   opt => opt.MapFrom(src => src.ProductVariants.Any()
                       ? src.ProductVariants.First().Price
                       : 0))
                
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
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.Size != null ? src.Size.SizeName : null))
            .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color != null ? src.Color.ColorCode : null)); 
            CreateMap<Warehouse, WarehouseResponse>();

            CreateMap<WarehouseRequest, Warehouse>()
                .ForMember(dest => dest.WarehouseId, opt => opt.Ignore()) 
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());
            CreateMap<ProductVariantRequest, ProductVariant>().ReverseMap();
        }
    }
}