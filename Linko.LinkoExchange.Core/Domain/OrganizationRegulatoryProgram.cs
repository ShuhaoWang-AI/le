﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    public class OrganizationRegulatoryProgram
    {
        public int OrganizationRegulatoryProgramId { get; set; }
        public int RegulatoryProgramId { get; set; }
        public Organization Organization { get; set; }
        public bool IsEnabled { get; set; }
        public List<OrganizationRegulatoryProgramSetting> OrganizationRegulatoryProgramSettings  { get; set; }
    }
}