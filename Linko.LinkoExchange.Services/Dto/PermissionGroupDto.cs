using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Dto
{
    public class PermissionGroupDto
    {
        public int PermissionGroupId
        {
            get;set;
        }

        public string Name
        {
            get;set;
        }

        public string Description
        {
            get;set;
        }

        public int OrganizationRegulatoryProgramId
        {
            get;set;
        }
        public DateTimeOffset CreationDateTimeUtc
        {
            get; set;
        }
        public DateTimeOffset? LastModificationDateTimeUtc
        {
            get; set;
        }
    }
}
