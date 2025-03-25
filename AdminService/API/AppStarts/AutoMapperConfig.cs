using AutoMapper;
using Domain.DTO.Response;
using Domain.Entities;

namespace API.AppStarts
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            //CreateMap<Account, AccountDTO>();
            CreateMap<Import, InventoryImportResponseDto>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedByNavigation.FullName))
            .ForMember(dest => dest.CreatedByEmail, opt => opt.MapFrom(src => src.CreatedByNavigation.Email))
            .ForMember(dest => dest.CreatedByPhone, opt => opt.MapFrom(src => src.CreatedByNavigation.PhoneNumber))
            .ForMember(dest => dest.CreatedByAddress, opt => opt.MapFrom(src => src.CreatedByNavigation.Address));

            CreateMap<Import, InventoryImportDetailDto>()
                .ForMember(dest => dest.CreatedByName,
                           opt => opt.MapFrom(src => src.CreatedByNavigation.FullName))
                .ForMember(dest => dest.Details,
                           opt => opt.MapFrom(src => src.ImportDetails));

            // Mapping từ entity ImportDetail sang InventoryImportDetailItemDto
            CreateMap<ImportDetail, InventoryImportDetailItemDto>()
                .ForMember(dest => dest.ProductVariantName,
                           opt => opt.MapFrom(src => src.ProductVariant.Product.Name))
                .ForMember(dest => dest.StoreDetails,
                           opt => opt.MapFrom(src => src.ImportStoreDetails));

            // Mapping từ entity ImportStoreDetail sang InventoryImportStoreDetailDto
            CreateMap<ImportStoreDetail, InventoryImportStoreDetailDto>()
                .ForMember(dest => dest.StoreId,
                           opt => opt.MapFrom(src => src.WareHouse.WarehouseId))
                .ForMember(dest => dest.StoreName,
                           opt => opt.MapFrom(src => src.WareHouse.WarehouseName))
                .ForMember(dest => dest.StaffName,
                           opt => opt.MapFrom(src => src.StaffDetail != null ? src.StaffDetail.Account.FullName : null));
        }
    }
}