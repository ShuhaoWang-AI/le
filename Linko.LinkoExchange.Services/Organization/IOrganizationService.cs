using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Organization
{
    public interface IOrganizationService
    {
        /// <summary>
        /// Get organizations that a user can access to
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>Collection of organization</returns>
        IEnumerable<OrganizationDto> GetUserOrganizations(int userId);

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
        void UpdateEnableDisableFlag(int orgRegProgId, bool isEnabled);

        List<OrganizationRegulatoryProgramDto> GetChildOrganizationRegulatoryPrograms(int orgRegProgId);

        void AddChildOrganization(int parentRegOrdId, OrganizationDto childOrganization);  
    }
}
