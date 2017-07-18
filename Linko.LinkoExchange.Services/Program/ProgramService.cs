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
            var userProfile = _linkoExchangeDbContext.Users.SingleOrDefault(u => u.Email == email) ?? _linkoExchangeDbContext.Users.SingleOrDefault(u => u.OldEmailAddress == email);
            if (userProfile == null) {
                return null;
            }

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

                    //Check if any of the authorities up the chain are disabled, making all it's IU's disabled too
                    var traversedAuthorityList = GetTraversedAuthorityList(u.OrganizationRegulatoryProgramDto.OrganizationRegulatoryProgramId);
                    if (traversedAuthorityList.Any(authority => !authority.IsEnabled))
                    {
                        u.OrganizationRegulatoryProgramDto.IsEnabled = false;
                    }

                }

            }
            return organziationRegulatoryProgramUserDtos;
        }

        public IEnumerable<OrganizationRegulatoryProgramDto> GetTraversedAuthorityList(int orgRegProgramId, List<OrganizationRegulatoryProgramDto> authorityList = null)
        {
            if (authorityList == null)
                authorityList = new List<Dto.OrganizationRegulatoryProgramDto>();

            var orgRegProgram = _linkoExchangeDbContext.OrganizationRegulatoryPrograms
                .SingleOrDefault(orp => orp.OrganizationRegulatoryProgramId == orgRegProgramId);

            if (orgRegProgram != null && orgRegProgram.RegulatorOrganizationId != null)
            {
                //Fetch authority
                var authority = _linkoExchangeDbContext.OrganizationRegulatoryPrograms
                    .SingleOrDefault(orp => orp.OrganizationId == orgRegProgram.RegulatorOrganizationId
                        && orp.RegulatoryProgramId == orgRegProgram.RegulatoryProgramId);

                if (authority != null)
                {
                    //prevent circular references by checking if authority exists in list already
                    if (!authorityList.Any(orp => orp.OrganizationRegulatoryProgramId == authority.OrganizationRegulatoryProgramId))
                    {
                        var authorityDto = _mapHelper.GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(authority);
                        authorityList.Add(authorityDto);
                        return GetTraversedAuthorityList(authority.OrganizationRegulatoryProgramId, authorityList);
                    }
                }

            }

            return authorityList;
        }

        public IEnumerable<OrganizationRegulatoryProgramDto> GetChildOrganizationRegulatoryPrograms(int currentOrganizationRegulatoryProgramId, string searchString)
        {
            throw new NotImplementedException();
        }
    }
}