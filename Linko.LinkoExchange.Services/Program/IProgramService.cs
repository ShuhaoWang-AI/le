using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Program
{
    public interface IProgramService
    {
        IEnumerable<OrganizationRegulatoryProgramUserDto> GetUserRegulatoryPrograms(string email);
        /// <summary>
        /// Get programs that a user can access
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>A collection of programs</returns>
        IEnumerable<ProgramDto> GetUserRegulatoryPrograms(int userId); 

        /// <summary>
        /// Get the organziation regulatory program 
        /// </summary>
        /// <param name="organizationRegulatoryProgramId">The organziation regulatory program Id</param>
        /// <returns></returns>
        OrganizationRegulatoryProgramDto GetOrganizationRegulatoryProgram(int organizationRegulatoryProgramId);

        OrganizationRegulatoryProgramUserDto CreateOrganizationRegulatoryProgramForUser(int userProfileId, int regulatoryProgramId);
    }
}
