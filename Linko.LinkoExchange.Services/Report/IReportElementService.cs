using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Report
{
    public interface IReportElementService
    {
        IEnumerable<CertificationTypeDto> GetCertificationTypes();
        void SaveCertificationType(CertificationTypeDto certType);
        void DeleteCertificationType(int certificationTypeId);
    }
}
