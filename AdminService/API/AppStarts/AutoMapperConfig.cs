using Application.DTO.Request;
using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using static Domain.DTO.Response.ProductDetailDTO;

namespace API.AppStarts
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<Import, ImportDto>();
            // Map từ ImportDetail sang ImportDetailDto
            CreateMap<ImportDetail, ImportDetailDto>();
            // Nếu có mapping cho StoreDetail cũng cần khai báo
            CreateMap<ImportStoreDetail, ImportStoreDetailDto>();

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
                           opt => opt.MapFrom(src => src.ImportDetails))
                   .ForMember(dest => dest.AuditLogs, opt => opt.Ignore()); // Vì ta gán sau khi lấy dữ liệu audit log;

            // Mapping từ entity ImportDetail sang InventoryImportDetailItemDto
            CreateMap<ImportDetail, InventoryImportDetailItemDto>()
     .ForMember(dest => dest.ProductVariantName,
         opt => opt.MapFrom(src => $"{src.ProductVariant.Product.Name} - {src.ProductVariant.Size.SizeName} - {src.ProductVariant.Color.ColorName}"))
     .ForMember(dest => dest.StoreDetails,
         opt => opt.MapFrom(src => src.ImportStoreDetails));

            // Mapping từ entity ImportStoreDetail sang InventoryImportStoreDetailDto
            CreateMap<ImportStoreDetail, InventoryImportStoreDetailDto>()
    .ForMember(dest => dest.StoreId,
               opt => opt.MapFrom(src => src.Warehouse.WarehouseId))
    .ForMember(dest => dest.StoreName,
               opt => opt.MapFrom(src => src.Warehouse.WarehouseName))
    .ForMember(dest => dest.StaffName,
               opt => opt.MapFrom(src => src.StaffDetail != null ? src.StaffDetail.Account.FullName : null))
             .ForMember(dest => dest.ActualQuantity,
               opt => opt.MapFrom(src => src.ActualReceivedQuantity));



            //opt => opt.MapFrom(src => src.StaffDetail != null ? src.StaffDetail.Account.FullName : null))
            //    .ForMember(dest => dest.ActualQuantity,
            //               opt => opt.MapFrom(src => src.ActualReceivedQuantity));
            CreateMap<Account, UserRequestDTO>().ReverseMap();
            CreateMap<Account, BanUserRequestDTO>().ReverseMap();
            CreateMap<Account, CreateUserRequestWithPasswordDTO>().ReverseMap();


            CreateMap<Role, RoleRequestDTO>().ReverseMap();
            CreateMap<Role, RoleCreateRequestDTO>().ReverseMap();
            CreateMap<AuditLog, AuditLogRes>().ForMember(dest => dest.ChangedByName,
               opt => opt.MapFrom(src => src.ChangedByNavigation.FullName));
             
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


            // Mapping từ CreateTransferFullFlowDto sang Transfer
            CreateMap<CreateTransferFullFlowDto, Transfer>()
                .ForMember(dest => dest.TransferDetails, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());
            CreateMap<CreateTransferFullFlowDto, Import>()
    .ForMember(dest => dest.ImportId, opt => opt.Ignore());
            // Mapping từng chi tiết chuyển hàng
            CreateMap<CreateTransferDetailDto, TransferDetail>();

            // Mapping từ Transfer sang TransferFullFlowDto
            CreateMap<Transfer, TransferFullFlowDto>()
                .ForMember(dest => dest.TransferOrderId, opt => opt.MapFrom(src => src.TransferOrderId))
                // Giả sử bạn lưu ImportId và DispatchId trong Transfer nếu cần
                .ForMember(dest => dest.ImportId, opt => opt.MapFrom(src => src.ImportId))
                .ForMember(dest => dest.DispatchId, opt => opt.MapFrom(src => src.DispatchId));
            //mapping transfer
            CreateMap<TransferDetail, TransferDetailDto>();
            CreateMap<TransferDto, TransferResponseDto>()
                           .ForMember(dest => dest.ImportReferenceNumber,
                                      opt => opt.MapFrom(src => src.ImportReferenceNumber))
                           .ForMember(dest => dest.DispatchReferenceNumber,
                                      opt => opt.MapFrom(src => src.DispatchReferenceNumber))
                            .ForMember(dest => dest.CreatedByName,
                           opt => opt.MapFrom(src => src.CreatedByName));


            //================Dispatch============
            CreateMap<Dispatch, JSONDispatchDTO>()
                .ForMember(dest => dest.CreatedByUser,
                           opt => opt.MapFrom(src => src.CreatedByNavigation.FullName))
                .ForMember(dest => dest.Details,
                           opt => opt.MapFrom(src => src.DispatchDetails)).ReverseMap();

            // Mapping DispatchDetail -> JSONDispatchDetailDTO
            CreateMap<DispatchDetail, JSONDispatchDetailDTO>()
                .ForMember(dest => dest.VariantName,
                           opt => opt.MapFrom(src => src.Variant.Product.Name))
                    .ForMember(dest => dest.SizeName,
                           opt => opt.MapFrom(src => src.Variant.Size.SizeName))
                           .ForMember(dest => dest.ColorName,
                           opt => opt.MapFrom(src => src.Variant.Color.ColorName))
                .ForMember(dest => dest.PriceProductVariant,
           opt => opt.MapFrom(src => src.Variant.Price))
                .ForMember(dest => dest.StoreExportDetail,
                           opt => opt.MapFrom(src => src.StoreExportStoreDetails)).ReverseMap(); //
            CreateMap<StoreExportStoreDetail, JSONStoreExportDetailDTO>()
                .ForMember(dest => dest.WarehouseName,
                           opt => opt.MapFrom(src => src.Warehouse.WarehouseName))
                .ForMember(dest => dest.Staff,
                           opt => opt.MapFrom(src => src.StaffDetail.Account.FullName))
                .ForMember(dest => dest.HandleBy,
                           opt => opt.MapFrom(src => src.HandleByNavigation.Account.FullName))


                
                .ReverseMap();

            CreateMap<StoreExportStoreDetail, JSONStoreExportStoreDetailByIdHandlerDTO>()
                .ForMember(dest => dest.WarehouseName,
                           opt => opt.MapFrom(src => src.Warehouse.WarehouseName))
                .ForMember(dest => dest.Staff,
                           opt => opt.MapFrom(src => src.StaffDetail.Account.FullName))
                .ForMember(dest => dest.HandleBy,

                           opt => opt.MapFrom(src => src.HandleByNavigation.Account.FullName))
                .ForMember(dest => dest.ReferenceNumber,
                           opt => opt.MapFrom(src => src.DispatchDetail.Dispatch.ReferenceNumber))
                 .ForMember(dest => dest.ProductName,
                           opt => opt.MapFrom(src => src.DispatchDetail.Variant.Product.Name))
                    .ForMember(dest => dest.SizeName,
                           opt => opt.MapFrom(src => src.DispatchDetail.Variant.Size.SizeName))
                           .ForMember(dest => dest.ColorName,
                           opt => opt.MapFrom(src => src.DispatchDetail.Variant.Color.ColorName))

                   .ForMember(dest => dest.WarehouseDestinationName,
                           opt => opt.MapFrom(src => src.Warehouse.WarehouseName)).ReverseMap(); 


            // Import ========
            CreateMap<ImportStoreDetail, JSONImportStoreDetailDTO>()
                .ForMember(dest => dest.Staff,
                           opt => opt.MapFrom(src => src.StaffDetail.Account.FullName))
                .ForMember(dest => dest.WarehouseName,
                           opt => opt.MapFrom(src => src.Warehouse.WarehouseName))
                .ForMember(dest => dest.HandleBy,
                           opt => opt.MapFrom(src => src.HandleByNavigation.Account.FullName))
                .ForMember(dest => dest.ReferenceNumber,
                           opt => opt.MapFrom(src => src.ImportDetail.Import.ReferenceNumber))
                .ForMember(dest => dest.CostPrice,
                           opt => opt.MapFrom(src => src.ImportDetail.CostPrice))
                .ForMember(dest => dest.ProductName,
                           opt => opt.MapFrom(src => src.ImportDetail.ProductVariant.Product.Name))
                .ForMember(dest => dest.SizeName,
                           opt => opt.MapFrom(src => src.ImportDetail.ProductVariant.Size.SizeName))
                .ForMember(dest => dest.ColorName,
                           opt => opt.MapFrom(src => src.ImportDetail.ProductVariant.Color.ColorName))
                .ForMember(dest => dest.AuditLogs, opt => opt.Ignore()).ReverseMap();
            //Iport json
            CreateMap<Import, JSONImportDTO>()
                .ForMember(dest => dest.CreatedBy,
                           opt => opt.MapFrom(src => src.CreatedByNavigation.FullName))
                .ForMember(dest => dest.Details,
                           opt => opt.MapFrom(src => src.ImportDetails)).ReverseMap();
            CreateMap<ImportDetail, JSONImportDetailDTO>()
                .ForMember(dest => dest.Product,
                           opt => opt.MapFrom(src => src.ProductVariant.Product.Name))
                    .ForMember(dest => dest.Size,
                           opt => opt.MapFrom(src => src.ProductVariant.Size.SizeName))
                        .ForMember(dest => dest.Color,
                           opt => opt.MapFrom(src => src.ProductVariant.Color.ColorName))
                .ForMember(dest => dest.PriceProductVariant,
                           opt => opt.MapFrom(src => src.ProductVariant.Price))
                .ForMember(dest => dest.StoreImportDetail,
                           opt => opt.MapFrom(src => src.ImportStoreDetails)).ReverseMap(); //
            CreateMap<ImportStoreDetail, JSONImportStoreDetailGetDTO>()
                .ForMember(dest => dest.Staff,
                           opt => opt.MapFrom(src => src.StaffDetail.Account.FullName))
                .ForMember(dest => dest.WarehouseName,
                           opt => opt.MapFrom(src => src.Warehouse.WarehouseName))
                .ForMember(dest => dest.HandleBy,
                           opt => opt.MapFrom(src => src.HandleByNavigation.Account.FullName)).ReverseMap();

            // Transfer
            CreateMap<Transfer, JSONTransferOrderDTO>()
                .ForMember(dest => dest.CreatedBy,
                           opt => opt.MapFrom(src => src.Import.CreatedByNavigation.FullName))
                .ForMember(dest => dest.DetailsTransferOrder,
                           opt => opt.MapFrom(src => src.TransferDetails)).ReverseMap();

            CreateMap<TransferDetail, JSONTransferOrderDetailDTO>()
                .ForMember(dest => dest.Product,
                           opt => opt.MapFrom(src => src.Variant.Product.Name))
                  .ForMember(dest => dest.Size,
                           opt => opt.MapFrom(src => src.Variant.Color.ColorName))
              .ForMember(dest => dest.Color,
                           opt => opt.MapFrom(src => src.Variant.Size.SizeName)).ReverseMap();




            CreateMap<Product, ProductDto>()
            // map ImagePath từ bộ ProductImages
            .ForMember(dest => dest.ImagePath,
                       opt => opt.MapFrom(src => src.ProductImages
                                                   .FirstOrDefault(pi => pi.IsMain)
                                                   .ImagePath));

            CreateMap<Product, ProductDto>()
    .ForMember(dest => dest.ImagePath,
               opt => opt.MapFrom(src => src.ProductImages.Select(img => img.ImagePath).ToList()));
            CreateMap<ProductVariant, ProductVariantDto>().ReverseMap();

            CreateMap<ProductEditDto, Product>()
            .ForMember(dest => dest.ProductImages, opt => opt.Ignore()); // Bỏ qua Images vì bạn xử lý riêng

            
          

        }
    }
}       
