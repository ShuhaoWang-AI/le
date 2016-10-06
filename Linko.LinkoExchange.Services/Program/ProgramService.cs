using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Program
{
    public class ProgramService : IProgramService
    {
        /// <summary>
        /// Get programs that a user can access
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>A collection of programs</returns>
        public IEnumerable<ProgramDto> GetUserPrograms(int userId)
        {
            var list = new List<ProgramDto>
            {
                new ProgramDto
                {
                    ProgramId = 1000,
                    ProgramName = "Mock program name"
                }
            };

            return list;
        }

        /// <summary>
        /// The programs that fullfilled an organization
        /// </summary>
        /// <param name="organizationId">Organization Id</param>
        /// <returns>A collection of programs</returns>
        public IEnumerable<ProgramDto> GetOrganization(int organizationId)
        {
            return new[]
            {
                new ProgramDto
                {
                    ProgramId = 1000,
                    ProgramName = "Mock program name"
                }
            };
        }
    }
}