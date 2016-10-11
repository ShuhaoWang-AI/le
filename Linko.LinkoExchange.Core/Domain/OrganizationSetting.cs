﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    public class OrganizationSetting
    {
        [Key]
        public int OrganizationSettingId { get; set;}
        public int OrganizationId { get; set;}
        public string Name { get; set;}
        public string Value { get; set;}
    }
}
