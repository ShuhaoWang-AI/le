using AutoMapper;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Extensions;
using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Linko.LinkoExchange.Services.AutoMapperProfile
{
    public class UserQuestionAnswerMapProfile : Profile
    {
        public UserQuestionAnswerMapProfile()
        {
            CreateMap<QuestionDto, Question>()
               .ForMember(d => d.QuestionType, o => o.Ignore())
               .ForMember(d => d.QuestionTypeId, o => o.MapFrom(i => (int)i.QuestionType))
               .ForMember(d => d.CreationDateTimeUtc, o => o.Ignore())
               .ForMember(d => d.LastModificationDateTimeUtc, o => o.MapFrom(s => DateTime.UtcNow))
               .ForMember(d => d.LastModifierUserId, o => o.MapFrom(s => HttpContext.Current != null ? HttpContext.Current.User.Identity.GetOrganizationRegulatoryProgramUserId() : (int?)null));

            CreateMap<AnswerDto, UserQuestionAnswer>()
                .ForMember(d => d.UserProfile, o => o.Ignore())
                .ForMember(d => d.UserProfileId, o => o.Ignore())
                .ForMember(d => d.CreationDateTimeUtc, o => o.Ignore())
                .ForMember(d => d.QuestionId, o => o.Ignore())
                .ForMember(d => d.Question, o => o.Ignore())
                .ForMember(d => d.LastModificationDateTimeUtc, o => o.MapFrom(s => DateTime.UtcNow));
        }
    }
}
