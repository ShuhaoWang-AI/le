using Linko.LinkoExchange.Services.Dto;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Permission
{
    public interface IPermissionService
    {
        /// <summary>
        /// Returns a collection of approvers for a given user across all org reg programs.
        /// </summary>
        /// <param name="userProfileId"></param>
        /// <returns></returns>
        IEnumerable<UserDto> GetAllAuthoritiesApprovalPeopleForUser(int userProfileId);

        /// <summary>
        /// Returns a collection of approvers for a given user within a given org reg program.
        /// </summary>
        /// <param name="organizationRegulatoryProgram"></param>
        /// <param name="isInvitedToIndustry"></param>
        /// <returns></returns>
        IEnumerable<UserDto> GetApprovalPeople(OrganizationRegulatoryProgramDto organizationRegulatoryProgram, bool isInvitedToIndustry);

        /// <summary>
        /// Returns the permission groups allowable for a given org reg program.
        /// </summary>
        /// <param name="orgRegProgramId"></param>
        /// <returns></returns>
        IEnumerable<PermissionGroupDto> GetRoles(int orgRegProgramId);
    }

}
