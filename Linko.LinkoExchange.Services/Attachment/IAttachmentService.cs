using Linko.LinkoExchange.Services.Dto;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services
{
    public interface IAttachmentService
    {
        List<AttachmentTypeDto> GetAttachmentTypes();
        List<CertificationTypeDto> GetCertificationTypes();
    }
}
