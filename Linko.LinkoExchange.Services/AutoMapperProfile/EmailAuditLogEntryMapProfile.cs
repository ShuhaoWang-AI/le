using AutoMapper;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.AutoMapperProfile
{
    /// <summary>
    /// AutoMapper profile from EmailAuditLogEntryDto between EmailAuditLog.
    /// </summary>
    public class EmailAuditLogEntryMapProfile : Profile
    {
        public EmailAuditLogEntryMapProfile()
        {
            CreateMap<EmailAuditLogEntryDto, EmailAuditLog>()
                .ForMember(i => i.EmailAuditLogId, o => o.Ignore());
            CreateMap<EmailAuditLog, EmailAuditLogEntryDto>() 
                .ForMember(i => i.EmailAuditLogId, o => o.Ignore());
        }  
    }
}