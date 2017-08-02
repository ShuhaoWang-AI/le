using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Organization
{
    public interface IOrganizationService
    {
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
        IEnumerable<AuthorityDto> GetUserRegulators(int userId, bool isIncludeRemoved = false);

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

        /// <summary>
        /// Returns the org reg program object associated with the passed in id
        /// </summary>
        /// <param name="orgRegProgId">tOrganizationRegulatoryProgramId.OrganizationRegulatoryProgramId</param>
        /// <param name="isAuthorizationRequired"></param>
        /// <returns></returns>
        OrganizationRegulatoryProgramDto GetOrganizationRegulatoryProgram(int orgRegProgId, bool isAuthorizationRequired = false);

        /// <summary>
        /// Returns a collection of industry org reg programs that have as their regulator the passed org reg program.
        /// Optionally filters the collection using a string "contains" searcg against the following fields:
        ///     - Organization name
        ///     - Organization street address lines
        ///     - Organization city
        ///     - Organization postal/zip code
        /// </summary>
        /// <param name="orgRegProgId"></param>
        /// <param name="searchString"></param>
        /// <returns></returns>
        List<OrganizationRegulatoryProgramDto> GetChildOrganizationRegulatoryPrograms(int orgRegProgId, string searchString = null);

        /// <summary>
        /// Returns the difference between the maximum allowable number of users and current user count.
        /// </summary>
        /// <param name="orgRegProgramId"></param>
        /// <returns></returns>
        int GetRemainingUserLicenseCount(int orgRegProgramId);

        /// <summary>
        /// Returns the difference between the maximum allowable child industries and the current industry count.
        /// </summary>
        /// <param name="orgRegProgramId"></param>
        /// <returns></returns>
        int GetRemainingIndustryLicenseCount(int orgRegProgramId);

        /// <summary>
        /// Returns the number of org reg program users within the passed in org reg program.
        /// </summary>
        /// <param name="orgRegProgramId"></param>
        /// <returns></returns>
        int GetCurrentUserLicenseCount(int orgRegProgramId);

        /// <summary>
        /// Returns the number of org reg programs that have the passed in org reg program as their regulator.
        /// </summary>
        /// <param name="orgRegProgramId"></param>
        /// <returns></returns>
        int GetCurrentIndustryLicenseCount(int orgRegProgramId);

        /// <summary>
        /// Gets the regulator of the passed in org reg program. In the case where there is no regulator,
        /// the passed in org reg program itself is returned.
        /// </summary>
        /// <param name="orgRegProgramId"></param>
        /// <returns></returns>
        OrganizationRegulatoryProgramDto GetAuthority(int orgRegProgramId);
    }
}
