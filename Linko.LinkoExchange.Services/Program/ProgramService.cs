using System;
using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.User;
using System.Linq;
using AutoMapper;
using Linko.LinkoExchange.Core.Domain;

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
        public IEnumerable<ProgramDto> GetUserRegulatoryPrograms(int userId)
        {
            var list = new List<ProgramDto>
            {
                new ProgramDto
                {
                    RegulatoryProgramId = 1000,
                   Name = "Mock program name"
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
                    RegulatoryProgramId = 1000,
                    Name = "Mock program name"
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
        /// Get all the OrganizationRegulatoryProgramDto(s) that the users have access to
        /// </summary>
        /// <param name="email">The email address.</param>
        /// <returns></returns>
        public IEnumerable<OrganizationRegulatoryProgramUserDto> GetUserRegulatoryPrograms(string  email)
        {
            //TODO 

//            var t = new OrganizationRegulatoryProgramUserDto
//             {
//                UserProfileDto = new UserDto
//                {
//                    FirstName = "test first name",
//                    LastName = "test last name",
//                    UserName = "the user name"
//                }
//            };
//
//            var list = new List<OrganizationRegulatoryProgramUserDto>();
//            list.Add(t);
//
//            return list;

           

            var userProfile = _userService.GetUserProfileByEmail(email);
            if (userProfile == null)
                return null;

            var organziationRegulatoryProgramUserDtos = new List<OrganizationRegulatoryProgramUserDto>();
            var regulatoryProgramUsers = _linkoExchangeDbContext
                .OrganizationRegulatoryProgramUsers.ToList()
                .FindAll(i => !i.IsRemoved && i.UserProfileId == userProfile.UserProfileId);
            if (regulatoryProgramUsers.Any()) 
            {
                organziationRegulatoryProgramUserDtos
                    .AddRange(regulatoryProgramUsers.Select(user => _mapper.Map<OrganizationRegulatoryProgramUserDto>(user)));

                foreach(var u in organziationRegulatoryProgramUserDtos)
                {
                    u.UserProfileDto = _userService.GetUserProfileById(u.UserProfileId);
                }

            }
            return organziationRegulatoryProgramUserDtos;
        }

        public OrganizationRegulatoryProgramUserDto CreateOrganizationRegulatoryProgramForUser(int userProfileId, int organizationRegulatoryProgramId)
        {
            var orpu = new OrganizationRegulatoryProgramUser();
            orpu.IsEnabled = true;
            orpu.IsRegistrationApproved = false;
            orpu.IsRegistrationDenied = false;
            orpu.IsSignatory = false;
            orpu.UserProfileId = userProfileId;
            //orpu.LastModificationDateTimeUtc = DateTime.UtcNow;
            orpu.CreationDateTimeUtc = DateTimeOffset.UtcNow;
            orpu.RegistrationDateTime = DateTimeOffset.UtcNow;
            orpu.OrganizationRegulatoryProgramId = organizationRegulatoryProgramId;

            _linkoExchangeDbContext.OrganizationRegulatoryProgramUsers.Add(orpu);
            _linkoExchangeDbContext.SaveChanges();

            return _mapper.Map<OrganizationRegulatoryProgramUserDto>(orpu);

        }
    }
}