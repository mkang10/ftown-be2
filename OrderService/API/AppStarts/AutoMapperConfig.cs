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
            CreateMap<Order, OrderResponse>()
            .ForMember(dest => dest.OrderTotal, opt => opt.MapFrom(src => src.OrderTotal ?? 0))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderDetails));

            CreateMap<OrderDetail, OrderItemResponse>()
                .ForMember(dest => dest.ProductVariantId, opt => opt.MapFrom(src => src.ProductVariantId))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.PriceAtPurchase, opt => opt.MapFrom(src => src.PriceAtPurchase));
            
            CreateMap<ShippingAddress, Order>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.District, opt => opt.MapFrom(src => src.District))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(dest => dest.Province, opt => opt.MapFrom(src => src.Province))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.RecipientName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.RecipientPhone));


            CreateMap<OrderItemResponse, ReturnItemResponse>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.PriceAtPurchase)); // Map giá lúc mua
                                                                                            //.ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.PriceAtPurchase * src.Quantity)) // Tổng giá tiền
                                                                                            //.ForMember(dest => dest.ReturnReason, opt => opt.Ignore()) // Không có sẵn trong OrderItemResponse
                                                                                            //.ForMember(dest => dest.ReturnOption, opt => opt.Ignore()); // Không có sẵn trong OrderItemResponse

            CreateMap<UpdateGHNIdDTO, Order>().ReverseMap();


        }
    }
}