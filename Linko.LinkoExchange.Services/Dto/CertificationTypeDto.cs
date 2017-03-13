
using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class CertificationTypeDto : ReportElementTypeDto
    {
        public int? CertificationTypeID { get; set; }
        public string CertificationText { get; set; }
    }

}