using System.Collections;
using System.Collections.Generic;

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

        public UserDto UserProfileDto
        {
            get; set;
        }

        public ICollection<OrganizationRegulatoryProgramDto> OrganizationRegulatoryProgramDtos
        {
            get; set;
        }
    }
}
