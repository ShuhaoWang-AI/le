using System;
using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Data;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Core.Enum;

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
        /// <param name="isIncludeRemoved">True if we want to include org reg programs the user has been removed from (for logging purposes)</param>
        /// <returns></returns>
        public IEnumerable<OrganizationRegulatoryProgramUserDto> GetUserRegulatoryPrograms(string email, bool isIncludeRemoved = false)
        {
            var userProfile = _linkoExchangeDbContext.Users.SingleOrDefault(u => u.Email == email);
            if (userProfile == null)
                return null;

            var organziationRegulatoryProgramUserDtos = new List<OrganizationRegulatoryProgramUserDto>();
            var regulatoryProgramUsers = _linkoExchangeDbContext
                .OrganizationRegulatoryProgramUsers
                .Include("OrganizationRegulatoryProgram").ToList()
                .FindAll(i => i.UserProfileId == userProfile.UserProfileId);

            if (!isIncludeRemoved)
            {
                regulatoryProgramUsers = regulatoryProgramUsers.FindAll(users => !users.IsRemoved);
            }

            if (regulatoryProgramUsers.Any())
            {
                organziationRegulatoryProgramUserDtos
                    .AddRange(regulatoryProgramUsers.Select(user => _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user)));

                foreach (var u in organziationRegulatoryProgramUserDtos)
                {
                    u.UserProfileDto = _mapHelper.GetUserDtoFromUserProfile(_linkoExchangeDbContext.Users.SingleOrDefault(user => user.UserProfileId == u.UserProfileId));

                    //Check if the authority is disabled, making all it's IU's disabled too
                    if (u.OrganizationRegulatoryProgramDto.IsEnabled && u.OrganizationRegulatoryProgramDto.RegulatorOrganizationId != null)
                    {
                        //Disable if it's authority is disabled
                        var regulatorOrganization = _linkoExchangeDbContext.OrganizationRegulatoryPrograms
                            .Single(orp => orp.OrganizationId == u.OrganizationRegulatoryProgramDto.RegulatorOrganizationId
                                && orp.RegulatoryProgramId == u.OrganizationRegulatoryProgramDto.RegulatoryProgramId);

                        u.OrganizationRegulatoryProgramDto.IsEnabled = regulatorOrganization.IsEnabled;
                    }
                    
                }

            }
            return organziationRegulatoryProgramUserDtos;
        }

        public OrganizationRegulatoryProgramUserDto CreateOrganizationRegulatoryProgramForUser(int userProfileId, int organizationRegulatoryProgramId, int inviterOrganizationRegulatoryProgramId, RegistrationType registrationType)
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
                //orpu.LastModificationDateTimeUtc = DateTimeOffset.Now;
                orpu.IsRemoved = false;
                orpu.CreationDateTimeUtc = DateTimeOffset.Now;
                orpu.RegistrationDateTimeUtc = DateTimeOffset.Now;
                orpu.OrganizationRegulatoryProgramId = organizationRegulatoryProgramId;
                orpu.InviterOrganizationRegulatoryProgramId = inviterOrganizationRegulatoryProgramId;

                _linkoExchangeDbContext.OrganizationRegulatoryProgramUsers.Add(orpu);
            }
            else
            {
                // To update the existing one.  
                orpu.IsRegistrationApproved = false;
                orpu.IsRegistrationDenied = false;
                orpu.IsRemoved = false;

                //Update to new re-reg time-stamp
                orpu.RegistrationDateTimeUtc = DateTimeOffset.Now;

                //Update because the new "Inviter" is now the Authority
                //(need to do this so that this pending registration show up under the Authority)
                orpu.InviterOrganizationRegulatoryProgramId = inviterOrganizationRegulatoryProgramId;

                //RESET SCENARIO
                if (registrationType == RegistrationType.ResetRegistration)
                {
                    // Update all other OrgRegProgm  IsRegistrationApproved to be false to enforce all the user be approved again by all program administrators where they were approved before 
                    var orpus = _linkoExchangeDbContext.OrganizationRegulatoryProgramUsers.Where
                        (i =>
                             i.UserProfileId == userProfileId &&
                             i.OrganizationRegulatoryProgramId != organizationRegulatoryProgramId &&
                             i.IsRemoved == false && i.IsRegistrationApproved).ToList();

                    foreach (var prog in orpus)
                    {
                        prog.IsRegistrationApproved = false;
                    }
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