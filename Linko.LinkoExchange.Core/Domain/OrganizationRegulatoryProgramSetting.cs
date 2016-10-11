﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    public class OrganizationRegulatoryProgramSetting
    {
        [Key]
        public int OrganizationRegulatoryProgramSettingId { get; set;}
        public string Name { get; set;}
        public string Value { get; set;}
    }
}
