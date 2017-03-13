using System;
using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Data;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Mapping;

namespace Linko.LinkoExchange.Services.Program
{
    public class ProgramService : IProgramService
    {
        private readonly LinkoExchangeContext _linkoExchangeDbContext;
        private readonly IMapHelper _mapHelper;

        public ProgramService(
            LinkoExchangeContext applicationDbContext,
            IMapHelper mapHelper
            )
        {
            _linkoExchangeDbContext = applicationDbContext;
            _mapHelper = mapHelper;
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
            return orp.Select(i =>
            {
                return _mapHelper.GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(i.OrganizationRegulatoryProgram);
            });
        }

        public OrganizationRegulatoryProgramDto GetOrganizationRegulatoryProgram(int organizationRegulatoryProgramId)
        {
            var orp = _linkoExchangeDbContext.OrganizationRegulatoryPrograms.SingleOrDefault(
                i => i.OrganizationRegulatoryProgramId == organizationRegulatoryProgramId && i.IsRemoved == false);

            return orp == null ? null : _mapHelper.GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(orp);
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
                    .AddRange(regulatoryProgramUsers.Select(user => _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user)));

                foreach (var u in organziationRegulatoryProgramUserDtos)
                {
                    u.UserProfileDto = _mapHelper.GetUserDtoFromUserProfile(_linkoExchangeDbContext.Users.SingleOrDefault(user => user.UserProfileId == u.UserProfileId));
                }

            }
            return organziationRegulatoryProgramUserDtos;
        }

        public OrganizationRegulatoryProgramUserDto CreateOrganizationRegulatoryProgramForUser(int userProfileId, int organizationRegulatoryProgramId, int inviterOrganizationRegulatoryProgramId)
        {
            //var orpu = new OrganizationRegulatoryProgramUser();
            var orpu = _linkoExchangeDbContext.OrganizationRegulatoryProgramUsers.SingleOrDefault
                (i => i.UserProfileId == userProfileId &&
                 i.OrganizationRegulatoryProgramId == organizationRegulatoryProgramId);
            if (orpu == null)
            {
                // To create a new OrgRegProgram
                orpu = new OrganizationRegulatoryProgramUser();
                orpu.IsEnabled = true;
                orpu.IsRegistrationApproved = false;
                orpu.IsRegistrationDenied = false;
                orpu.IsSignatory = false;
                orpu.UserProfileId = userProfileId;
                //orpu.LastModificationDateTimeUtc = DateTime.UtcNow;
                orpu.IsRemoved = false;
                orpu.CreationDateTimeUtc = DateTimeOffset.UtcNow;
                orpu.RegistrationDateTimeUtc = DateTimeOffset.UtcNow;
                orpu.OrganizationRegulatoryProgramId = organizationRegulatoryProgramId;
                orpu.InviterOrganizationRegulatoryProgramId = inviterOrganizationRegulatoryProgramId;

                _linkoExchangeDbContext.OrganizationRegulatoryProgramUsers.Add(orpu);
            }
            else
            {
                // To update the exsiting one.  
                orpu.IsRegistrationApproved = false;
                orpu.IsRegistrationDenied = false;
                orpu.IsRemoved = false;

                //Update to new re-reg timestamp
                orpu.RegistrationDateTimeUtc = DateTimeOffset.UtcNow;

                //RESET SCENARIO
                //Update because the new "Inviter" is now the Authority
                //(need to do this so that this pending registration show up under the Authority)
                orpu.InviterOrganizationRegulatoryProgramId = inviterOrganizationRegulatoryProgramId;

                // Update all other OrgRegProgm  IsRegistrationApproved to be false to enforce all the user be approved again by all program administrators 
                var orpus = _linkoExchangeDbContext.OrganizationRegulatoryProgramUsers.Where
                        (i => i.UserProfileId == userProfileId && i.OrganizationRegulatoryProgramId != organizationRegulatoryProgramId && i.IsRemoved == false).ToList();

                foreach (var prog in orpus)
                {
                    prog.IsRegistrationApproved = false;
                }
            }

            _linkoExchangeDbContext.SaveChanges();

            return _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(orpu);

        }

        public IEnumerable<OrganizationRegulatoryProgramDto> GetChildOrganizationRegulatoryPrograms(int currentOrganizationRegulatoryProgramId, string searchString)
        {
            throw new NotImplementedException();
        }
    }
}