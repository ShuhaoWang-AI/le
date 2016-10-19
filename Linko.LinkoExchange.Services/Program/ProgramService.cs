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

        public OrganizationRegulatoryProgramDto GetOrganizationRegulatoryProgram(int organizationRegulatoryProgramId)
        {
            var  orp =  _linkoExchangeDbContext.OrganizationRegulatoryPrograms.SingleOrDefault(
                i => i.OrganizationRegulatoryProgramId == organizationRegulatoryProgramId);

            return orp == null ? null : _mapper.Map<OrganizationRegulatoryProgramDto>(orp);
        }

        /// <summary>
        /// Get all the OrganizationRegulatoryProgram(s) that the users have access to
        /// </summary>
        /// <param name="email">The email address.</param>
        /// <returns></returns>
        public IEnumerable<OrganizationRegulatoryProgramUserDto> GetUserPrograms(string  email)
        {
            var userProfile = _userService.GetUserProfileByEmail(email);
            if (userProfile == null)
                return null;

            var organziationRegulatoryProgramUserDtos = new List<OrganizationRegulatoryProgramUserDto>();
            var regulatoryProgramUsers = _linkoExchangeDbContext.OrganizationRegulatoryProgramUsers.ToList().FindAll(i => i.UserProfileId == userProfile.UserProfileId);
            if (regulatoryProgramUsers.Any()) 
            {
                organziationRegulatoryProgramUserDtos
                    .AddRange(regulatoryProgramUsers.Select(user => _mapper.Map<OrganizationRegulatoryProgramUserDto>(user)));
            }
            return organziationRegulatoryProgramUserDtos;
        }
    }
}