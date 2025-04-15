using AutoMapper;
using Domain.DTO.Request;
using Domain.Entities;

namespace API.AppStarts
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            //CreateMap<Account, AccountDTO>();
            CreateMap<Conversation, ConversationCreateRequest>().ReverseMap();
            CreateMap<Conversation, ConversationRequest>().ReverseMap();

            CreateMap<Message, MessageCreateRequest>().ReverseMap();
            CreateMap<Message, MessageRequest>().ReverseMap();
        }
    }
}