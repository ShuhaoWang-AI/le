using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Program
{
    public interface IProgramService
    {
        /// <summary>
        /// Get programs that a user can access
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>A collection of programs</returns>
        IEnumerable<ProgramDto> GetUserPrograms(int userId);

        /// <summary>
        /// The programs that fulfilled an organization
        /// </summary>
        /// <param name="organizationId">Organization Id</param>
        /// <returns>A collection of programs</returns>
        IEnumerable<ProgramDto> GetOrganization(int organizationId);
    }
}
