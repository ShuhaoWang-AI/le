using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services
{

    public class ReportPakcageTemplateDto
    {
        public int Id { get; set; }
        public string TemplateName { get; set; }
        public string Descriptioin { get; set; }
        public DateTimeOffset? EffectiveDate { get; set; }
        public UserProfile CreatedBu { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public UserProfile LastModifiedBy { get; set; }
        public DateTimeOffset? LastModifiedDate { get; set; }
        public CtsEventTypeDto CTSEventType { get; set; }
        public List<AttachmentDto> Attachments { get; set; }
        public List<OrganizationRegulatoryProgramDto> AssignedIndustries { get; set; }
    }
}
