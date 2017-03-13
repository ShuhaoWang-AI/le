
using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class CertificationTypeDto : ReportElementCategoryDto
    {
        public int? CertificationTypeID { get; set; }
        public string CertificationTypeName { get; set; }

        public string CertificationTypeDescription { get; set; }
        public CtsEventTypeDto CTSEventType { get; set; }
        public string CertificationText { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }

    }

}