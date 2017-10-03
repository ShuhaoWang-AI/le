using System.Collections.Generic;
using System.Linq;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;

namespace Linko.LinkoExchange.Services.Program
{
    public class ProgramService : IProgramService
    {
        #region fields

        private readonly LinkoExchangeContext _linkoExchangeDbContext;
        private readonly IMapHelper _mapHelper;

        #endregion

        #region constructors and destructor

        public ProgramService(
            LinkoExchangeContext applicationDbContext,
            IMapHelper mapHelper
        )
        {
            _linkoExchangeDbContext = applicationDbContext;
            _mapHelper = mapHelper;
        }

        #endregion

        #region interface implementations

        /// <summary>
        ///     Get programs that a user can access
        /// </summary>
        /// <param name="userId"> User id </param>
        /// <returns> A collection of programs </returns>
        public IEnumerable<OrganizationRegulatoryProgramDto> GetUserRegulatoryPrograms(int userId)
        {
            var orp = _linkoExchangeDbContext.OrganizationRegulatoryProgramUsers.Include(path:"OrganizationRegulatoryProgram")
                                             .Where(i => i.IsRemoved == false && i.OrganizationRegulatoryProgram.IsRemoved == false && i.UserProfileId == userId).ToList();
            return orp.Select(i => _mapHelper.GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(orgRegProgram:i.OrganizationRegulatoryProgram));
        }

        public OrganizationRegulatoryProgramDto GetOrganizationRegulatoryProgram(int organizationRegulatoryProgramId)
        {
            var orp = _linkoExchangeDbContext.OrganizationRegulatoryPrograms.SingleOrDefault(
                                                                                             i => i.OrganizationRegulatoryProgramId == organizationRegulatoryProgramId
                                                                                                  && i.IsRemoved == false);

            return orp == null ? null : _mapHelper.GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(orgRegProgram:orp);
        }

        /// <summary>
        ///     Get all the OrganizationRegulatoryProgramDto(s) that the users have access to
        /// </summary>
        /// <param name="email"> The email address. </param>
        /// <param name="isIncludeRemoved"> True if we want to include org reg programs the user has been removed from (for logging purposes) </param>
        /// ///
        /// <param name="isIncludeDisabled"> True if we want to include org reg programs the user has been disabled from (for logging purposes) </param>
        /// <returns> </returns>
        public IEnumerable<OrganizationRegulatoryProgramUserDto> GetUserRegulatoryPrograms(string email, bool isIncludeRemoved = false, bool isIncludeDisabled = false)
        {
            var userProfile = _linkoExchangeDbContext.Users.SingleOrDefault(u => u.Email == email)
                              ?? _linkoExchangeDbContext.Users.SingleOrDefault(u => u.OldEmailAddress == email);
            if (userProfile == null)
            {
                return null;
            }

            var organziationRegulatoryProgramUserDtos = new List<OrganizationRegulatoryProgramUserDto>();
            var regulatoryProgramUsers = _linkoExchangeDbContext
                .OrganizationRegulatoryProgramUsers
                .Include(path:"OrganizationRegulatoryProgram").ToList()
                .FindAll(i => i.UserProfileId == userProfile.UserProfileId);

            if (!isIncludeRemoved)
            {
                regulatoryProgramUsers = regulatoryProgramUsers.FindAll(users => !users.IsRemoved);
            }

            if (!isIncludeDisabled)
            {
                regulatoryProgramUsers = regulatoryProgramUsers.FindAll(users => users.IsEnabled);
            }

            if (regulatoryProgramUsers.Any())
            {
                organziationRegulatoryProgramUserDtos
                    .AddRange(collection:regulatoryProgramUsers.Select(user => _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user:user)));

                foreach (var u in organziationRegulatoryProgramUserDtos)
                {
                    u.UserProfileDto =
                        _mapHelper.GetUserDtoFromUserProfile(userProfile:_linkoExchangeDbContext.Users.SingleOrDefault(user => user.UserProfileId == u.UserProfileId));

                    //Check if any of the authorities up the chain are disabled, making all it's IU's disabled too
                    var traversedAuthorityList = GetTraversedAuthorityList(orgRegProgramId:u.OrganizationRegulatoryProgramDto.OrganizationRegulatoryProgramId);
                    if (traversedAuthorityList.Any(authority => !authority.IsEnabled))
                    {
                        u.OrganizationRegulatoryProgramDto.IsEnabled = false;
                    }
                }
            }

            return organziationRegulatoryProgramUserDtos;
        }

        /// <inheritdoc />
        public ICollection<OrganizationRegulatoryProgramUserDto> GetActiveRegulatoryProgramUsers(string email, bool includeDisabled)
        {
            return GetUserRegulatoryPrograms(email:email, isIncludeDisabled:includeDisabled)
                .Where(i => i.OrganizationRegulatoryProgramDto.IsEnabled
                            && i.IsRegistrationApproved 
                            && i.IsRegistrationDenied == false
                            && !i.InviterOrganizationRegulatoryProgramDto.IsRemoved).ToList();
        }

        #endregion

        public IEnumerable<OrganizationRegulatoryProgramDto> GetTraversedAuthorityList(int orgRegProgramId, List<OrganizationRegulatoryProgramDto> authorityList = null)
        {
            if (authorityList == null)
            {
                authorityList = new List<OrganizationRegulatoryProgramDto>();
            }

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
                        var authorityDto = _mapHelper.GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(orgRegProgram:authority);
                        authorityList.Add(item:authorityDto);
                        return GetTraversedAuthorityList(orgRegProgramId:authority.OrganizationRegulatoryProgramId, authorityList:authorityList);
                    }
                }
            }

            return authorityList;
        }
    }
}