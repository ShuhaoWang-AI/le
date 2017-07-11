using Linko.LinkoExchange.Services.Dto;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Permission
{
    public interface IPermissionService
    {
        IEnumerable<UserDto> GetAllAuthoritiesApprovalPeopleForUser(int userProfileId);
        IEnumerable<UserDto> GetApprovalPeople(int organizationId);

        IEnumerable<PermissionGroupDto> GetRoles(int orgRegProgramId);
    }

}
