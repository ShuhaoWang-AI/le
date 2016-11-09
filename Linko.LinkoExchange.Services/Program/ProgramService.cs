using System;
using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Data;
using System.Linq;
using AutoMapper;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Services.Program
{
    public class ProgramService : IProgramService
    {
        private readonly LinkoExchangeContext _linkoExchangeDbContext;
        private readonly IMapper _mapper; 
        public ProgramService(
            LinkoExchangeContext applicationDbContext, 
            IMapper mapper
            )
        {
            _linkoExchangeDbContext = applicationDbContext;
            _mapper = mapper; 
        }
        
        /// <summary>
        /// Get programs that a user can access
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>A collection of programs</returns>
        public IEnumerable<OrganizationRegulatoryProgramDto> GetUserRegulatoryPrograms(int userId)
        {
            var orp = _linkoExchangeDbContext.OrganizationRegulatoryProgramUsers.Include("OrganizationRegulatoryProgram")
                                    .Where(i => i.IsRemoved == false && 
                                           i.OrganizationRegulatoryProgram.IsRemoved == false && 
                                           i.UserProfileId == userId).ToList();
            return orp.Select(i=>
            {
                return _mapper.Map<OrganizationRegulatoryProgramDto>(i);
            });
        }

        public OrganizationRegulatoryProgramDto GetOrganizationRegulatoryProgram(int organizationRegulatoryProgramId)
        {
            var  orp =  _linkoExchangeDbContext.OrganizationRegulatoryPrograms.SingleOrDefault(
                i => i.OrganizationRegulatoryProgramId == organizationRegulatoryProgramId && i.IsRemoved == false);

            return orp == null ? null : _mapper.Map<OrganizationRegulatoryProgramDto>(orp);
        }

        /// <summary>
        /// Get all the OrganizationRegulatoryProgramDto(s) that the users have access to
        /// </summary>
        /// <param name="email">The email address.</param>
        /// <returns></returns>
        public IEnumerable<OrganizationRegulatoryProgramUserDto> GetUserRegulatoryPrograms(string email)
        {
            var userProfile = _linkoExchangeDbContext.Users.SingleOrDefault(u => u.Email == email);
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
                    u.UserProfileDto = _mapper.Map<UserProfile, UserDto>(_linkoExchangeDbContext.Users.SingleOrDefault(user => user.UserProfileId == u.UserProfileId));
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
            orpu.RegistrationDateTimeUtc = DateTimeOffset.UtcNow;
            orpu.OrganizationRegulatoryProgramId = organizationRegulatoryProgramId;

            _linkoExchangeDbContext.OrganizationRegulatoryProgramUsers.Add(orpu);
            _linkoExchangeDbContext.SaveChanges();

            return _mapper.Map<OrganizationRegulatoryProgramUserDto>(orpu);

        }
    }
}