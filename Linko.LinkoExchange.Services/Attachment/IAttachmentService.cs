using Linko.LinkoExchange.Services.Dto;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Attachment
{
    interface IAttachmentService
    {
        List<AttachmentTypeDto> GetAttachmentTypes();
        List<CertificationTypeDto> GetCertificationTypes();
    }
}
