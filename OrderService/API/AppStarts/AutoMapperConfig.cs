using Application.DTO.Request;
using Application.DTO.Response;
using AutoMapper;
using Domain.Entities;
using Infrastructure.HelperServices.Models;

namespace API.AppStarts
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<Order, OrderResponse>()
            .ForMember(dest => dest.SubTotal, opt => opt.MapFrom(src => src.OrderTotal ?? 0))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderDetails));

            CreateMap<OrderDetail, OrderItemResponse>()
                .ForMember(dest => dest.OrderDetailId, opt => opt.MapFrom(src => src.OrderDetailId))
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
            CreateMap<PayOSCreateResult, CreatePaymentResponse>();

            CreateMap<OrderItemResponse, ReturnItemResponse>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.PriceAtPurchase)); // Map giá lúc mua
                                                                                            
            CreateMap<UpdateGHNIdDTO, Order>().ReverseMap();

            CreateMap<Order, InvoiceForEmailDTO>()
                .ForMember(dest => dest.OrderdetailEmail, opt => opt.MapFrom(src => src.OrderDetails)).ReverseMap();
            CreateMap<OrderDetail, OrderDetailEmailDTO>()
    .ForMember(dest => dest.Item, opt => opt.MapFrom(src => src.ProductVariant))
    .ReverseMap();
            CreateMap<ProductVariant, ProductDetailEmailDTO>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.SizeId, opt => opt.MapFrom(src => src.Size.SizeName))
                .ForMember(dest => dest.ColorId, opt => opt.MapFrom(src => src.Color.ColorName)).ReverseMap();



        }
    }
}