using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Organization
{
    public interface IOrganizationService
    {
        IEnumerable<OrganizationDto> GetUserOrganizationsByOrgRegProgUserId(int orgRegProgUserId);

        IEnumerable<OrganizationRegulatoryProgramDto> GetUserOrganizations();
        /// <summary>
        /// Get organizations that a user can access to (IU portal, AU portal, content MGT portal)
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>Collection of organization</returns>
        IEnumerable<OrganizationRegulatoryProgramDto> GetUserOrganizations(int userId);

        /// <summary>
        /// Get the regulators list that the user belonged to.
        /// </summary>
        /// <param name="userId">The user Id.</param>
        /// <returns></returns>
        IEnumerable<AuthorityDto> GetUserRegulators(int userId);

        string GetUserAuthorityListForEmailContent(int userProfileId);

            /// <summary>
        /// Get the organization by organization id
        /// </summary>
        /// <param name="organizationId">Organization id</param>
        /// <returns>Collection of organization</returns>
        OrganizationDto GetOrganization(int organizationId);

        /// When enabling, we need to check the parent (RegulatorOrganizationId)
        /// to see if there are any available licenses left
        ///
        /// Otherwise throw exception
        EnableOrganizationResultDto UpdateEnableDisableFlag(int orgRegProgId, bool isEnabled, bool isAuthorizationRequired = false);

        OrganizationRegulatoryProgramDto GetOrganizationRegulatoryProgram(int orgRegProgId, bool isAuthorizationRequired = false);

        List<OrganizationRegulatoryProgramDto> GetChildOrganizationRegulatoryPrograms(int orgRegProgId, string searchString = null);

        int GetRemainingUserLicenseCount(int orgRegProgramId);

        int GetRemainingIndustryLicenseCount(int orgRegProgramId);

        int GetCurrentUserLicenseCount(int orgRegProgramId);

        int GetCurrentIndustryLicenseCount(int orgRegProgramId);

        OrganizationRegulatoryProgramDto GetAuthority(int orgRegProgramId);
    }
}
