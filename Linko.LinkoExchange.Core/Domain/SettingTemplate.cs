using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    public class SettingTemplate
    {
        [Key]
        public int SettingTemplateId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DefaultValue { get; set; }

        public int OrganizationTypeId { get; set; }
        public OrganizationType OrganizationType { get; set; }
    }
}
