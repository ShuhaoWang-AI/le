using Linko.LinkoExchange.Core.Common;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Program;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Linko.LinkoExchange.Services.Permission
{
    public class PermissionService : IPermissionService
    {
        private readonly IProgramService _programService;
        private readonly LinkoExchangeContext _dbContext;
        private readonly IMapHelper _mapHelper; 

        public PermissionService(
            IProgramService programService, 
            LinkoExchangeContext dbContext,
            IMapHelper mapHelper)

        {
            if (programService == null) throw new ArgumentNullException("programService");
            if (dbContext == null) throw new ArgumentNullException("dbContext");

            _programService = programService;
            _dbContext = dbContext;
            _mapHelper = mapHelper; 
        }

        /// <summary>
        /// Get the user who has permission to approve this user.
        /// </summary>
        /// <param name="userProfileId">The UserProfileId of the user who is waiting for approval</param> 
        /// <returns></returns>
        public IEnumerable<UserDto> GetAllAuthoritiesApprovalPeopleForUser(int userProfileId)
        {
            // To determine if  the user is an authority user or industry user. 
            // For industry users, the approval people are authority stand users and authority admins
            // For authority users, the approval people are only  authority admin 
            
            var isAuthorityUser = IsAuthorityUser(userProfileId); 
             
            var authorityOrganizationIds = new List<int>();
            // step 1,  find all the OrganziationRegulatoryPrograms this user is in
            var orgRegPrograms  = _dbContext.OrganizationRegulatoryProgramUsers
                .Include(s => s.OrganizationRegulatoryProgram)
                .Where(u => u.UserProfileId == userProfileId &&
                            !u.IsRemoved &&
                            u.IsEnabled && 
                            !u.IsRemoved
                            )
                .Select(i=>i.OrganizationRegulatoryProgram).ToList();

            // step 2, find the authorities' organization Ids 
            foreach (var orgRegProgram in orgRegPrograms)
            {
                authorityOrganizationIds.Add(orgRegProgram.RegulatorOrganizationId ?? orgRegProgram.OrganizationId);
            }
            
            authorityOrganizationIds = authorityOrganizationIds.Distinct().ToList();

            // step 3,  find 'Administrators and standard users for all authorities' users 
            var approvals = _dbContext.OrganizationRegulatoryProgramUsers 
                .Include(i =>i.PermissionGroup)
                .Where(u => u.IsRemoved == false &&
                        authorityOrganizationIds.Contains(u.OrganizationRegulatoryProgram.OrganizationId) &&
                        u.IsRemoved == false &&
                        u.IsEnabled &&
                        u.IsRegistrationDenied == false &&
                        u.IsRegistrationApproved);

            if (isAuthorityUser)
            {
                 approvals = approvals.Where(u=> (u.PermissionGroup.Name == UserRole.Administrator.ToString()));
            }
            else
            {
                 approvals = approvals.Where(u=> (u.PermissionGroup.Name == UserRole.Standard.ToString() || u.PermissionGroup.Name == UserRole.Administrator.ToString()));
            }
            
            var userProfileIds =  approvals.Select(i=>i.UserProfileId).Distinct();
            
            var userProfiles = _dbContext.Users.Where(i => userProfileIds.Contains(i.UserProfileId) && 
                                i.IsAccountLocked == false && 
                                i.IsAccountResetRequired == false).ToList();
            var userDtos = userProfiles.Select(i => _mapHelper.GetUserDtoFromUserProfile(i));

            return userDtos;
        }
        
        public IEnumerable<UserDto> GetApprovalPeople(OrganizationRegulatoryProgramDto approverOrganizationRegulatoryProgram, bool isInvitedToIndustry) 
        {
            try
            {
                var isInviterOrgIndustry = approverOrganizationRegulatoryProgram.RegulatorOrganizationId.HasValue;

                var users = _dbContext.OrganizationRegulatoryProgramUsers.Include("PermissionGroup")
                                      .Where(u => u.IsRemoved == false &&
                                                  u.OrganizationRegulatoryProgram.IsEnabled && 
                                                  u.OrganizationRegulatoryProgram.IsRemoved == false && 
                                                  u.IsEnabled && u.IsRegistrationApproved &&
                                                  u.IsRegistrationDenied == false &&
                                                  u.OrganizationRegulatoryProgramId == approverOrganizationRegulatoryProgram.OrganizationRegulatoryProgramId);

                if (!isInvitedToIndustry)
                {
                    // if registering for authority then only administrators can approve or deny
                    users = users.Where(u => u.PermissionGroup.Name == UserRole.Administrator.ToString());
                }
                else if (isInviterOrgIndustry && isInvitedToIndustry)
                {
                    // if registering for industry and inviter organization is also industry then only administrators can approve or deny
                    users = users.Where(u => u.PermissionGroup.Name == UserRole.Administrator.ToString());
                }
                else
                {
                    // if registering for industry and inviter organization is authority then all user can approve or deny
                }

                var userProfileIds = users.Select(i => i.UserProfileId).Distinct();
                var userProfiles = _dbContext.Users.Where(i => userProfileIds.Contains(i.UserProfileId) &&
                                i.IsAccountLocked == false && 
                                i.IsAccountResetRequired == false 
                            ).ToList();
                var userDtos = userProfiles.Select(i => _mapHelper.GetUserDtoFromUserProfile(i));

                return userDtos;
            }
            catch (Exception ex)
            {
                var linkoException = new LinkoExchangeException();
                linkoException.ErrorType = LinkoExchangeError.OrganizationSetting;
                linkoException.Errors = new List<string> { ex.Message };
                throw linkoException;
            }
        }

        /// <summary>
        /// Returns the permission groups allowable for a given org reg program.
        /// </summary>
        /// <param name="orgRegProgramId"></param>
        /// <returns></returns>
        public IEnumerable<PermissionGroupDto> GetRoles(int orgRegProgramId)
        {
            var roleList = new List<PermissionGroupDto>();
            var roles = _dbContext.PermissionGroups.Where(p => p.OrganizationRegulatoryProgramId == orgRegProgramId);
            foreach (var role in roles)
            {
                var dto = _mapHelper.GetPermissionGroupDtoFromPermissionGroup(role);
                roleList.Add(dto);
            }

            return roleList;
        }


        /// <summary>
        /// Checks to see if the given user profile id belongs to at least one active and valid user of an authority.
        /// </summary>
        /// <param name="userProfileId"></param>
        /// <returns></returns>
        private bool IsAuthorityUser(int userProfileId)
        {
            return(_dbContext.OrganizationRegulatoryProgramUsers.Any(i=>
                i.OrganizationRegulatoryProgram.Organization.OrganizationType.Name == OrganizationTypeName.Authority.ToString() && 
                i.IsEnabled && 
                i.IsRegistrationDenied == false && 
                i.IsRemoved == false && 
                i.OrganizationRegulatoryProgram.IsEnabled && 
                i.UserProfileId == userProfileId
            ));
        }
    }
}
