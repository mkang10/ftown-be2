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
            CreateMap<Account, UserRequestDTO>().ReverseMap();
            CreateMap<Role, RoleRequestDTO>().ReverseMap();

        }
    }
}