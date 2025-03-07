using Application.DTO.Request;
using AutoMapper;
using Domain.Entities;

namespace API.AppStarts
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            //CreateMap<Account, AccountDTO>();
            CreateMap<Product, ProductExcelRequestDTO>().ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Category.Name));

        }
    }
}