using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class CertificationTypeDto
    {
        public int? CertificationTypeID { get; set; }
        public string CertificationTypeName { get; set; }

        public string CertificationTypeDescription { get; set; }
        public CTSEventTypeDto CTSEventType { get; set; }
        public string CertificationText { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }

    }

}