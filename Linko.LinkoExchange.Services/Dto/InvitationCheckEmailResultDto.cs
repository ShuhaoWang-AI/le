using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Dto
{
    public class InvitationCheckEmailResultDto
    {
        public OrganizationRegulatoryProgramUserDto ExistingUserSameProgram { get; set; }
        public OrganizationRegulatoryProgramUserDto ExistingUserDifferentProgram { get; set; }
    }
}
