using System;
using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.User;
using System.Linq;
using AutoMapper;

namespace Linko.LinkoExchange.Services.Program
{
    public class ProgramService : IProgramService
    {
        private readonly LinkoExchangeContext _linkoExchangeDbContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper; 
        public ProgramService(
            LinkoExchangeContext applicationDbContext, 
            IUserService userService,
            IMapper mapper
            )
        {
            _userService = userService;
            _linkoExchangeDbContext = applicationDbContext;
            _mapper = mapper; 
        }
        
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
        /// The programs that fulfilled by an organization
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

        /// <summary>
        /// Get all the OrganizationRegulatoryProgram(s) that the users have access to
        /// </summary>
        /// <param name="emails"></param>
        /// <returns></returns>
        public IEnumerable<ProgramDto> GetUserPrograms(string  email)
        {
            var user = _userService.GetUserProfileByEmail(email);
            if (user == null)
                return null;

            var regulatoryProgramUsers = _linkoExchangeDbContext.OrganizationRegulatoryProgramUsers.ToList().Find(i => i.UserProfileId == user.UserProfileId);  
            return null;
        }
    }
}