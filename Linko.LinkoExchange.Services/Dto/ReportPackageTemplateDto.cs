using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ReportPackageTemplateDto
    {
        public int? ReportPackageTemplateId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset EffectiveDateTimeUtc { get; set; }
        public DateTimeOffset? RetirementDateTimeUtc { get; set; }
        public bool IsSubmissionSignatoryRequired { get; set; }
        public int? CtsEventTypeId { get; set; }
        public CtsEventTypeDto CtsEventType { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreationDateTimeUtc { get; set; }
        public DateTimeOffset LastModificationDateTimeUtc { get; set; }
        public int? LastModifierUserId { get; set; }

        public List<ReportElementTypeDto> AttachmentTypes { get; set; }
        public List<ReportElementTypeDto> CertificationTypes { get; set; }
        public List<ReportPackageTemplateAssignmentDto> ReportPackageTemplateAssignments { get; set; }
        public List<ReportPackageTemplateElementCategoryDto> ReportPackageTemplateElementCategories { get; set; }
    }
}