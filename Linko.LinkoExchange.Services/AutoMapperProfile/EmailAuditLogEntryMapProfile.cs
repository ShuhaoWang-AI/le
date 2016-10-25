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
            CreateMap<EmailAuditLogEntryDto, EmailAuditLog>(); 
            CreateMap<EmailAuditLog, EmailAuditLogEntryDto>(); 
        }  
    } 
}