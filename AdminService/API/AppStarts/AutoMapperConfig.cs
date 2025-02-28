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
            CreateMap<Account, BanUserRequestDTO>().ReverseMap();
            CreateMap<Account, CreateUserRequestWithPasswordDTO>().ReverseMap();


            CreateMap<Role, RoleRequestDTO>().ReverseMap();
            CreateMap<Role, RoleCreateRequestDTO>().ReverseMap();

        }
    }
}