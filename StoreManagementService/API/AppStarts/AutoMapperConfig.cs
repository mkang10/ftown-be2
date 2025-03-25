using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.DTO.Response.Application.Imports.Dto;
using Domain.Entities;

namespace API.AppStarts
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            // --- Mapping từ Request DTO sang Entity ---
            CreateMap<CreateImportDto, Import>()
                .ForMember(dest => dest.ImportId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.TotalCost, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ImportDetails, opt => opt.MapFrom(src => src.ImportDetails));

            CreateMap<CreateImportDetailDto, ImportDetail>()
                .ForMember(dest => dest.ImportDetailId, opt => opt.Ignore())
                .ForMember(dest => dest.Import, opt => opt.Ignore()) // Không map navigation property để tránh vòng lặp
                .ForMember(dest => dest.ImportStoreDetails, opt => opt.MapFrom(src => src.StoreDetails));

            CreateMap<CreateStoreDetailDto, ImportStoreDetail>()
                .ForMember(dest => dest.ImportStoreId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"));

            // --- Mapping từ Entity sang Response DTO ---
            CreateMap<Import, ImportDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.CreatedByNavigation.Email))
                .ForMember(dest => dest.ImportDetails, opt => opt.MapFrom(src => src.ImportDetails))
                .ForMember(dest => dest.HandleByName, opt => opt.MapFrom(src =>
                    src.HandleByNavigation != null ? src.HandleByNavigation.Account.FullName : null))
                 .ForMember(dest => dest.HandleBy, opt => opt.MapFrom(src => src.HandleByNavigation.AccountId))
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src =>
                    src.CreatedByNavigation != null ? src.CreatedByNavigation.FullName : null));
            CreateMap<ImportDetail, ImportDetailDto>()
                .ForMember(dest => dest.ImportStoreDetails, opt => opt.MapFrom(src => src.ImportStoreDetails))
                // Loại bỏ property 'Import' (không map) để tránh vòng lặp:
                .ForSourceMember(src => src.Import, opt => opt.DoNotValidate());

            CreateMap<ImportStoreDetail, ImportStoreDetailDto>();
            CreateMap<ProductVariant, ProductVariantResponseDto>();

            //get all staff
            CreateMap<ImportStoreDetail, InventoryImportStoreDetailDto>()
                // Các property có cùng tên sẽ được map tự động:
                // ImportStoreId, ImportDetailId, WareHouseId, AllocatedQuantity, Status, Comments, StaffDetailId
                .ForMember(dest => dest.WareHouseName,
                           opt => opt.MapFrom(src => src.Warehouse.WarehouseName))
                .ForMember(dest => dest.ImportId, opt => opt.MapFrom(src => src.ImportDetail.Import.ImportId))

                .ForMember(dest => dest.StaffName,
                           opt => opt.MapFrom(src => src.StaffDetail != null ? src.StaffDetail.Account.FullName : null));


        }
    }
}
