using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{
    public class AttachmentTypeDto : ReportElementCategoryDto
    {
        public int ReportElementCategoryId { get; set; }
        public int? AttachmentTypeID { get; set; }
        public string AttachmentTypeName { get; set; }
        public string AttachmentTypeDescription { get; set; }
        public CtsEventTypeDto CtsEventType { get; set; }
        public bool? IsDeleted { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public DateTimeOffset CreationDateTimeUtc { get; set; }
        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }
        public int? LastModifierUserId { get; set; }
    }
}