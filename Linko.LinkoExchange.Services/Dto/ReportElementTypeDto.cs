﻿using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
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
        public ReportElementCategoryName ReportElementCategory { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public DateTime LastModificationDateTimeLocal { get; set; }
        public string LastModifierFullName { get; set; }
    }
}
