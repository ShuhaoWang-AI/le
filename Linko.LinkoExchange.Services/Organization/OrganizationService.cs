using Linko.LinkoExchange.Services.Dto;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Organization
{
    public class OrganizationService : IOrganizationService
    {
        /// <summary>
        /// Get organizations that a user can access to
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>Collection of organization</returns>
        public IEnumerable<OrganizationDto> GetUserOrganizations(int userId)
        {
            //TODO
            var list = new List<OrganizationDto>
            {
                new OrganizationDto
                {
                    OrganizationId = 1000,
                    OrganizationName = "Mock organization name"
                }
            };

            return list;
        }

        /// <summary>
        /// Get the organization by organization id
        /// </summary>
        /// <param name="organizationId">Organization id</param>
        /// <returns>Collection of organization</returns>
        public OrganizationDto GetOrganization(int organizationId)
        {
            //TODO
            return new OrganizationDto
            {
                OrganizationId = 1000,
                OrganizationName = "Mock organization name"
            };
        }

        public void UpdateOrganization(OrganizationDto organization)
        {
            //TODO
        }

        public void UpdateSettings(OrganizationSettingsDto settings)
        {
            //TODO
        }


        /// When enabling, we need to check the parent (RegulatorOrganizationId)
        /// to see if there are any available licenses left
        ///
        /// Otherwise throw exception
        public void UpdateEnableDisableFlag(int orgRegProgId, bool isEnabled)
        {
            //TODO
        }

        public List<OrganizationDto> GetChildrenOrganizations(int regOrgId)
        {
            return new List<OrganizationDto>();
        }

        public void AddChildOrganization(int parentRegOrdId, OrganizationDto childOrganization)
        {
            //TODO
        }

    }
}