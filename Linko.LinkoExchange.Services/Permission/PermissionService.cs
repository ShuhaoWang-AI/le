using Linko.LinkoExchange.Core.Common;
using Linko.LinkoExchange.Core.Domain;
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

        public PermissionService(IProgramService programService, LinkoExchangeContext dbContext,
            IMapHelper mapHelper)
        {
            if (programService == null) throw new ArgumentNullException("programService");
            if (dbContext == null) throw new ArgumentNullException("dbContext");

            _programService = programService;
            _dbContext = dbContext;
            _mapHelper = mapHelper;

        }

        public IEnumerable<UserDto> GetAllAuthoritiesApprovalPeopleForUser(int userProfileId)
        {
            var authorityOrganizationIds = new List<int>();
            // step 1,  find all the OrganziationRegulatoryPrograms this user is in
            var orgRegPrograms  = _dbContext.OrganizationRegulatoryProgramUsers
                .Include(s => s.OrganizationRegulatoryProgram)
                .Where(u => u.UserProfileId == userProfileId &&
                            !u.IsRemoved &&
                            u.IsEnabled &&
                            !u.IsRemoved
                            )
                .Select(i=>i.OrganizationRegulatoryProgram);

            // step 2, find the authorities' organization Ids
            foreach (var orgRegProgram in orgRegPrograms)
            {
                authorityOrganizationIds.Add(orgRegProgram.RegulatorOrganizationId ?? orgRegProgram.OrganizationId);
            }
            
            authorityOrganizationIds = authorityOrganizationIds.Distinct().ToList();

            // step 3,  find 'Administrators and standard users for all authorities'
            var userProfileIds = _dbContext.OrganizationRegulatoryProgramUsers 
                .Include(i =>i.PermissionGroup)
                .Where(u => u.IsRemoved == false &&
                            authorityOrganizationIds.Contains(u.OrganizationRegulatoryProgram.OrganizationId) &&
                            (u.PermissionGroup.Name == UserRole.Standard.ToString() ||
                             u.PermissionGroup.Name == UserRole.Administrator.ToString()))
                .Select(i=>i.UserProfileId).Distinct();
            
            var userProfiles = _dbContext.Users.Where(i => userProfileIds.Contains(i.UserProfileId)).ToList();
            var userDtos = userProfiles.Select(i => _mapHelper.GetUserDtoFromUserProfile(i));

            return userDtos;
        }

        // TODO: to implement 
        public IEnumerable<UserDto> GetApprovalPeople(int organizationRegulatoryProgramId)
        {
            try
            {
                var users = _dbContext.OrganizationRegulatoryProgramUsers.Include("PermissionGroup")
                .Where(u => u.IsRemoved == false &&
                            u.OrganizationRegulatoryProgramId == organizationRegulatoryProgramId &&
                            u.PermissionGroup.Name == UserRole.Administrator.ToString());

                var userProfileIds = users.Select(i => i.UserProfileId).Distinct();
                var userProfiles = _dbContext.Users.Where(i => userProfileIds.Contains(i.UserProfileId)).ToList();
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
    }
}
