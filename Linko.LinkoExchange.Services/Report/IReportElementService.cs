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
        void SaveCertificationType(CertificationTypeDto certType);
    }
}
