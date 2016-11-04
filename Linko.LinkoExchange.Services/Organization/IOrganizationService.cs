using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Organization
{
    public interface IOrganizationService
    {
        IEnumerable<OrganizationDto> GetUserOrganizationsByOrgRegProgUserId(int orgRegProgUserId);

        IEnumerable<OrganizationRegulatoryProgramDto> GetUserOrganizations();
        /// <summary>
        /// Get organizations that a user can access to
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

            /// <summary>
        /// Get the organization by organization id
        /// </summary>
        /// <param name="organizationId">Organization id</param>
        /// <returns>Collection of organization</returns>
        OrganizationDto GetOrganization(int organizationId);

        /// <summary>
        /// Update the Organization underneath the Authority or Industry
        /// Should not be called directly from BL layer
        /// </summary>
        /// <param name="organization"></param>
        void UpdateOrganization(OrganizationDto organization);


        /// When enabling, we need to check the parent (RegulatorOrganizationId)
        /// to see if there are any available licenses left
        ///
        /// Otherwise throw exception
        bool UpdateEnableDisableFlag(int orgRegProgId, bool isEnabled);

        OrganizationRegulatoryProgramDto GetOrganizationRegulatoryProgram(int orgRegProgId);

        List<OrganizationRegulatoryProgramDto> GetChildOrganizationRegulatoryPrograms(int orgRegProgId, string startsWith = null);

        void AddChildOrganization(int parentRegOrdId, OrganizationDto childOrganization);

        int GetRemainingUserLicenseCount(int orgRegProgramId, bool isForAuthority);

        int GetRemainingIndustryLicenseCount(int orgRegProgramId);

        OrganizationRegulatoryProgramDto GetAuthority(int orgRegProgramId);
    }
}
