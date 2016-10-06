using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Common
{
    public class OrganizationSettings
    {
        public IEnumerable<Setting> Settings
        {
            get;set;
        }

        public string OrganizationId
        {
            get;set;
        } 
    }
}
