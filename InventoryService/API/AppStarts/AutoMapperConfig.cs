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
                    opt => opt.MapFrom(src => src.ProductVariants.Any() ? src.ProductVariants.First().Price : 0))
                .ForMember(dest => dest.ImagePath,
                    opt => opt.MapFrom(src => src.ProductImages
                        .Where(pi => pi.IsMain)
                        .Select(pi => pi.ImagePath)
                        .FirstOrDefault()))
                .ForMember(dest => dest.Colors,
                    opt => opt.MapFrom(src =>
                        src.ProductVariants
                            .Where(pv => pv.Color != null && !string.IsNullOrEmpty(pv.Color.ColorCode))
                            .Select(pv => pv.Color.ColorCode)
                            .Distinct()
                            .ToList()
                    ));


            CreateMap<Color, ColorInfo>();

            CreateMap<CreateProductRequest, Product>()
            .ForMember(dest => dest.ProductImages, opt => opt.Ignore())  // Vì sẽ xử lý upload ảnh riêng
            .ForMember(dest => dest.Status, opt => opt.Ignore());
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
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Product.ProductId))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.Size != null ? src.Size.SizeName : null))
            .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color != null ? src.Color.ColorCode : null))
            .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src =>
                src.WareHousesStocks
                    .Where(ws => ws.WarehouseId == 2) // ✅ Lọc WarehouseId = 2
                    .Sum(ws => ws.StockQuantity))); // ✅ Tính tổng số lượng;
            CreateMap<Warehouse, WarehouseResponse>();

            CreateMap<WarehouseRequest, Warehouse>()
                .ForMember(dest => dest.WarehouseId, opt => opt.Ignore()) 
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());
            CreateMap<ProductVariantRequest, ProductVariant>().ReverseMap();

            CreateMap<Product, TopSellingProductResponse>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.ProductImages
                                                       .Where(i => i.IsMain)
                                                       .Select(i => i.ImagePath)
                                                       .FirstOrDefault()))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.Price, opt => opt.Ignore()) // sẽ set thủ công từ variant
            .ForMember(dest => dest.DiscountedPrice, opt => opt.Ignore())
            .ForMember(dest => dest.QuantitySold, opt => opt.Ignore())
            .ForMember(dest => dest.Revenue, opt => opt.Ignore())
            .ForMember(dest => dest.PromotionTitle, opt => opt.Ignore())
            .ForMember(dest => dest.Colors,
                        opt => opt.MapFrom(src =>
                            src.ProductVariants
                                .Where(pv => pv.Color != null && !string.IsNullOrEmpty(pv.Color.ColorCode))
                                .Select(pv => pv.Color.ColorCode!)
                                .Distinct()
                                .ToList()
                        ));
            CreateMap<ColorDTO, Color>().ReverseMap();
            CreateMap<CreateColorDTO, Color>().ReverseMap();
            CreateMap<SizeDTO, Size>().ReverseMap();
            CreateMap<CreateSizeDTO, Size>().ReverseMap();


        }
    }
}