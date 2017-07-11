using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{
    public class InvitationCheckEmailResultDto
    {
        public OrganizationRegulatoryProgramUserDto ExistingUserSameProgram { get; set; }
        public ICollection<OrganizationRegulatoryProgramUserDto> ExistingUsersDifferentPrograms { get; set; }
    }
}
