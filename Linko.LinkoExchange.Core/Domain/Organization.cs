using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    public class Organization
    {
        [Key]
        public int OrganizationId { get; set; }
        public int OrganizationTypeId { get; set; } 
        [Required]
        public virtual OrganizationType OrganizationType { get; set; }
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string CityName { get; set; }
        public string ZipCode { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneExt { get; set; }
        public string FaxNumber { get; set; }
        public string WebsiteURL { get; set; }
        public int JurisdictionId { get; set; }

        public virtual List<OrganizationSetting> OrganizationSettings { get; set; }
    }

}
