using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Dto
{
    public class OrganizationRegulatoryProgramDto
    { 
        public int OrganizationRegulatoryProgramId
        {
            get;set;
        }

        public int RegulatoryProgramId
        {
            get;set;
        }

        public int OrganizationId
        {
            get;set;
        }

        public int RegulatorOrganizationId
        {
            get;set;
        }

        public bool IsEnabled
        {
            get;set;
        }

        public bool IsRemoved
        {
            get;set;
        }

        public bool HasSignatory
        {
            get; set;
        }
    }
}
