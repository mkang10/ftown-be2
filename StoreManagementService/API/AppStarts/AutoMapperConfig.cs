using Application.UseCases;
using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.DTO.Response.Application.Imports.Dto;
using Domain.Entities;
using static Domain.DTO.Response.OrderAssigmentRes;
using static Domain.DTO.Response.OrderDoneRes;
using static Domain.DTO.Response.OrderDTO;

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
                .ForMember(dest => dest.ImportStoreDetails, opt => opt.MapFrom(src => src.StoreDetails))
                                .ForMember(dest => dest.CostPrice, opt => opt.MapFrom(src => src.CostPrice));
            ;


            CreateMap<CreateStoreDetailDto, ImportStoreDetail>()
                .ForMember(dest => dest.ImportStoreId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"));


            // Với SupplementImportRequestDto, ta dùng IncludeBase để kế thừa mapping từ CreateImportDto
            CreateMap<SupplementImportRequestDto, Import>()
             .ForMember(dest => dest.ImportId, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.Status, opt => opt.Ignore())
             .ForMember(dest => dest.TotalCost, opt => opt.Ignore())
             .ForMember(dest => dest.ApprovedDate, opt => opt.Ignore())
             .ForMember(dest => dest.CompletedDate, opt => opt.Ignore())
             .ForMember(dest => dest.ImportDetails, opt => opt.MapFrom(src => src.ImportDetails))
             .ForMember(dest => dest.OriginalImportId, opt => opt.MapFrom(src => src.OriginalImportId));

            CreateMap<SupplementImportDetailDto, ImportDetail>()
                .ForMember(dest => dest.ImportDetailId, opt => opt.Ignore())
                .ForMember(dest => dest.Import, opt => opt.Ignore()) // bỏ qua navigation property
                .ForMember(dest => dest.ImportStoreDetails, opt => opt.Ignore());

            // --- Mapping từ Entity sang Response DTO ---
            CreateMap<Import, ImportDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.CreatedByNavigation.Email))
                .ForMember(dest => dest.ImportDetails, opt => opt.MapFrom(src => src.ImportDetails))
               
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src =>
                    src.CreatedByNavigation != null ? src.CreatedByNavigation.FullName : null));
            CreateMap<ImportDetail, ImportDetailDto>()
                .ForMember(dest => dest.ImportStoreDetails, opt => opt.MapFrom(src => src.ImportStoreDetails))
                // Loại bỏ property 'Import' (không map) để tránh vòng lặp:
                .ForSourceMember(src => src.Import, opt => opt.DoNotValidate());

            CreateMap<ImportStoreDetail, ImportStoreDetailDto>().ForMember(dest => dest.HandleByName, opt => opt.MapFrom(src =>
                    src.HandleByNavigation != null ? src.HandleByNavigation.Account.FullName : null))
                 .ForMember(dest => dest.HandleBy, opt => opt.MapFrom(src => src.HandleByNavigation.AccountId));
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
            CreateMap<ProductVariant, ProductVariantResponseDto>()
              .ForMember(dest => dest.VariantId, opt => opt.MapFrom(src => src.VariantId))
              .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
              .ForMember(dest => dest.SizeName, opt => opt.MapFrom(src => src.Size == null ? string.Empty : src.Size.SizeName))
              .ForMember(dest => dest.ColorName, opt => opt.MapFrom(src => src.Color == null ? string.Empty : src.Color.ColorName))
              .ForMember(dest => dest.MainImagePath, opt => opt.MapFrom(src =>
                  src.Product.ProductImages.Where(pi => pi.IsMain).Select(pi => pi.ImagePath).FirstOrDefault()));

             CreateMap<Dispatch, DispatchResponseDto>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedByNavigation.FullName))
            .ForMember(dest => dest.DispatchDetails, opt => opt.MapFrom(src => src.DispatchDetails));

        CreateMap<DispatchDetail, DispatchDetailDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src =>
                src.Variant.Product.Name + " " +
                src.Variant.Color.ColorName + " " +
                src.Variant.Size.SizeName))
            .ForMember(dest => dest.ExportDetails, opt => opt.MapFrom(src => src.StoreExportStoreDetails));

        CreateMap<StoreExportStoreDetail, ExportDetailDto>()
            .ForMember(dest => dest.StaffName, opt => opt.MapFrom(src => src.StaffDetail != null ? src.StaffDetail.Account.FullName : null))
            .ForMember(dest => dest.DispatchId, opt => opt.MapFrom(src => src.DispatchDetail != null ? src.DispatchDetail.DispatchId : 0))
;
            CreateMap<Order, OrderDTO>();
            CreateMap<OrderAssignment, OrderAssignmentDTO>();

            CreateMap<AssignStaffDTO, OrderAssignment>()
            .ForMember(dest => dest.StaffId, opt => opt.MapFrom(src => src.StaffId))
            .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments));

            CreateMap<OrderAssignment, OrderAssignmentResponseDTO>();
            CreateMap<Order, OrderResponseDTO>();

        }





    }
}
