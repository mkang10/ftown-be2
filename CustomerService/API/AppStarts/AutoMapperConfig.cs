using Application.DTO.Request;
using AutoMapper;
using Domain.Entities;

namespace API.AppStarts
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<Feedback, FeedbackRequestDTO>().ReverseMap();
            CreateMap<Feedback, CreateFeedBackRequestDTO>().ReverseMap();
            CreateMap<Feedback, UpdateFeedbackRequestDTO>().ReverseMap();

            CreateMap<ReplyFeedback, ReplyRequestDTO>().ReverseMap();
            CreateMap<ReplyFeedback, CreateReplyRequestDTO>().ReverseMap();
            CreateMap<ReplyFeedback, UpdateReplyRequestDTO>().ReverseMap();

        }
    }
}