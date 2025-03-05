using Application.DTO.Request;
using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;

namespace API.AppStarts
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            //import
            CreateMap<CreateInventoryImportDto, InventoryImport>()
             .ForMember(dest => dest.InventoryImportHistories, opt => opt.MapFrom(src => src.Histories))
             .ForMember(dest => dest.InventoryImportDetails, opt => opt.MapFrom(src => src.Details));

            CreateMap<CreateInventoryImportDetailDto, InventoryImportDetail>()
                .ForMember(dest => dest.InventoryImportStoreDetails, opt => opt.MapFrom(src => src.StoreAllocations));

            CreateMap<CreateInventoryImportStoreDetailDto, InventoryImportStoreDetail>();
            // Ánh xạ cho GetAll response
            CreateMap<InventoryImport, InventoryImportResponseDto>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedByNavigation.FullName));

            CreateMap<CreateInventoryImportHistoryDto, InventoryImportHistory>();

            //export
            CreateMap<CreateInventoryTransactionDto, InventoryTransaction>()
            .ForMember(dest => dest.InventoryTransactionDetails, opt => opt.MapFrom(src => src.Details))
            .ForMember(dest => dest.InventoryTransactionHistories, opt => opt.MapFrom(src => src.Histories))
            .ForMember(dest => dest.Documents, opt => opt.MapFrom(src => src.Documents));

            CreateMap<CreateInventoryTransactionDetailDto, InventoryTransactionDetail>();
            CreateMap<CreateInventoryTransactionHistoryDto, InventoryTransactionHistory>();
            CreateMap<CreateDocumentDto, Document>();
        }
    }
}