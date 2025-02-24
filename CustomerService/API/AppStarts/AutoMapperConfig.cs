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
            // Mapping từ Account -> CustomerProfileResponse
            CreateMap<Account, CustomerProfileResponse>()
                .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.ImagePath))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.LastLoginDate, opt => opt.MapFrom(src => src.LastLoginDate))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            // Mapping từ CustomerDetail -> CustomerProfileResponse
            CreateMap<CustomerDetail, CustomerProfileResponse>()
                .ForMember(dest => dest.LoyaltyPoints, opt => opt.MapFrom(src => src.LoyaltyPoints))
                .ForMember(dest => dest.MembershipLevel, opt => opt.MapFrom(src => src.MembershipLevel))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.CustomerType, opt => opt.MapFrom(src => src.CustomerType))
                .ForMember(dest => dest.PreferredPaymentMethod, opt => opt.MapFrom(src => src.PreferredPaymentMethod));
            // Chuyển đổi Entity → DTO
            CreateMap<CartItem, CartItemResponse>()
                .ForMember(dest => dest.Price, opt => opt.Ignore()); // Lấy giá từ DB sau

            CreateMap<ShoppingCart, ShoppingCartResponseDto>();

            // Chuyển đổi DTO → Entity
            CreateMap<AddToCartRequest, CartItem>();
        }
    }
}