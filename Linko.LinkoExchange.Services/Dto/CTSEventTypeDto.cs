
using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class CtsEventTypeDto
    {
        public int CtsEventTypeId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string CtsEventCategoryName { get; set; }

        public int OrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgramDto OrganizationRegulatoryProgram { get; set; }

        public bool IsRemoved { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }
    }
}