using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ReportElementTypeDto
    {
        public int? ReportElementTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public bool IsContentProvided { get; set; }
        public CtsEventTypeDto CtsEventType { get; set; }
        public int ReportElementCategoryId { get; set; }
        public ReportElementCategoryDto ReportElementCategory { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public OrganizationRegulatoryProgramDto OrganizationRegulatoryProgram { get; set; }
        public DateTimeOffset CreationDateTimeUtc { get; set; }
        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }
        public int? LastModifierUserId { get; set; }
    }
}
