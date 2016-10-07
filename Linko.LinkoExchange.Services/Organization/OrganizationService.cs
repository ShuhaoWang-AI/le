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
        public IEnumerable<Dto.OrganizationDto> GetUserOrganizations(int userId)
        {
            //TODO
            var list = new List<Dto.OrganizationDto>
            {
                new Dto.OrganizationDto
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
        public Dto.OrganizationDto GetOrganization(int organizationId)
        {
            //TODO
            return new Dto.OrganizationDto
            {
                OrganizationId = 1000,
                OrganizationName = "Mock organization name"
            };
        }

        public void UpdateOrganization(OrganizationDto organization)
        {
            //TODO
        }

    }
}