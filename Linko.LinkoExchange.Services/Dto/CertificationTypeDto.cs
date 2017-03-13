
using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class CertificationTypeDto : ReportElementCategoryDto
    {
        public int ReportElementCategoryId { get; set; }
        public int? CertificationTypeID { get; set; }
        public string CertificationTypeName { get; set; }

        public string CertificationTypeDescription { get; set; }
        public CtsEventTypeDto CtsEventType { get; set; }
        public string CertificationText { get; set; }
        public bool? IsDeleted { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public DateTimeOffset CreationDateTimeUtc { get; set; }
        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }
        public int? LastModifierUserId { get; set; }

    }

}