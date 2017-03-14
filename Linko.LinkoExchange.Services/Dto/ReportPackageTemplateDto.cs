using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ReportPackageTemplateDto
    {
        public int? Id { get; set; }
        public string TemplateName { get; set; }
        public string Description { get; set; }
        public DateTimeOffset EffectiveDate { get; set; }
        public UserProfile CreatedBu { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public UserProfile LastModifiedBy { get; set; }
        public DateTimeOffset LastModifiedDate { get; set; }
        public CtsEventTypeDto CtsEventType { get; set; }
        public List<AttachmentTypeDto> AttachmentTypes { get; set; }
        public List<CertificationTypeDto> CertificationTypes { get; set; }
        public List<ReportPackageTemplateAssignmentDto> ReportPackageTemplateAssignments { get; set; }
        public List<ReportPackageTemplateElementCategoryDto> ReportPackageTemplateElementCategories { get; set; }
        public int Content { get; set; }
        public bool IsContentProvided { get; set; }
        public int SortOrder { get; set; }
    }
}