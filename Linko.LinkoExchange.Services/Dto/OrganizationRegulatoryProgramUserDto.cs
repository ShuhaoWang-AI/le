using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Dto
{
    public class OrganizationRegulatoryProgramUserDto
    {
        public int UserProfileId
        {
            get;set;
        }

        public int OragnizationRegulatoryProgramId
        {
            get;set;
        }

        public bool IsEnabled
        {
            get;set;
        } 
    }
}
