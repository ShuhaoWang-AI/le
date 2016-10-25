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

        public int OrganizationRegulatoryProgramId
        {
            get;set;
        }

        public bool IsEnabled
        {
            get;set;
        }

        public bool IsRegistrationApproved { get; set; }
        public bool IsRegistrationDenied { get; set; }
        public bool IsRemoved { get; set; } 
        public PermissionGroupDto PermissionGroup
        {
            get;set;
        }
        public UserDto UserProfileDto
        {
            get; set;
        }

        public OrganizationRegulatoryProgramDto OrganizationRegulatoryProgramDto
        {
            get; set;
        }
    }
}
