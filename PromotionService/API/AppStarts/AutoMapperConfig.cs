using Application.DTO.Request;
using Application.DTO.Response;
using AutoMapper;
using Domain.Entities;
using Newtonsoft.Json;

namespace API.AppStarts
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            // CreatePromotionRequest -> Promotion (Entity)
            CreateMap<CreatePromotionRequest, Promotion>()
                .ForMember(dest => dest.ApplyValue, opt => opt.MapFrom(src =>
                    src.ApplyValue != null ? JsonConvert.SerializeObject(src.ApplyValue) : null));

            // UpdatePromotionRequest -> Promotion (Entity)
            CreateMap<UpdatePromotionRequest, Promotion>()
                .ForMember(dest => dest.ApplyValue, opt => opt.MapFrom(src =>
                    src.ApplyValue != null ? JsonConvert.SerializeObject(src.ApplyValue) : null));

            // Promotion (Entity) -> PromotionResponse
            CreateMap<Promotion, PromotionResponse>()
                .ForMember(dest => dest.ApplyValue, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.ApplyValue)
                    ? JsonConvert.DeserializeObject<List<int>>(src.ApplyValue)
                    : new List<int>()));

            // Promotion (Entity) -> PromotionListResponse
            CreateMap<Promotion, PromotionListResponse>();
        }
    }
}