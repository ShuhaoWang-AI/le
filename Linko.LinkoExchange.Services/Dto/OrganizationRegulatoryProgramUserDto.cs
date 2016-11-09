using System;
using System.Collections;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{
    public class OrganizationRegulatoryProgramUserDto
    {
        public int OrganizationRegulatoryProgramUserId { get; set; }
        public int UserProfileId { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsRegistrationApproved { get; set; }
        public bool IsRegistrationDenied { get; set; }
        public bool IsRemoved { get; set; }
        public bool IsSignatory { get; set; }
        public PermissionGroupDto PermissionGroup { get; set; }
        public DateTimeOffset? RegistrationDateTimeUtc { get; set; }
        public UserDto UserProfileDto { get; set; }
        public virtual OrganizationRegulatoryProgramDto OrganizationRegulatoryProgramDto { get; set; }
    }
}
