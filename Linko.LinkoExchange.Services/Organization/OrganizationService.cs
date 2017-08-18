using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Extensions;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.Jurisdiction;
using System.Data.Entity;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Cache;
using System.Runtime.CompilerServices;
using Linko.LinkoExchange.Services.Base;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.TimeZone;

namespace Linko.LinkoExchange.Services.Organization
{
    public class OrganizationService : BaseService, IOrganizationService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly ISettingService _settingService;
        private readonly IHttpContextService _httpContext;
        private readonly IJurisdictionService _jurisdictionService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IMapHelper _mapHelper;

        public OrganizationService(LinkoExchangeContext dbContext, ISettingService settingService,
             IHttpContextService httpContext, IJurisdictionService jurisdictionService, ITimeZoneService timeZoneService,
            IMapHelper mapHelper)
        {
            _dbContext = dbContext;
            _settingService = settingService;
            _httpContext = httpContext;
            _jurisdictionService = jurisdictionService;
            _timeZoneService = timeZoneService;
            _mapHelper = mapHelper;
        }

        public override bool CanUserExecuteApi([CallerMemberName] string apiName = "", params int[] id)
        {
            bool retVal = false;

            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var currentOrgRegProgUserId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId));
            var currentOrganizationId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationId));
            var currentRegulatoryProgramId = _dbContext.OrganizationRegulatoryPrograms.Single(orp => orp.OrganizationRegulatoryProgramId == currentOrgRegProgramId).RegulatoryProgramId;
            var currentPortalName = _httpContext.GetClaimValue(CacheKey.PortalName);
            currentPortalName = string.IsNullOrWhiteSpace(value: currentPortalName) ? "" : currentPortalName.Trim().ToLower();

            switch (apiName)
            {
                case "GetOrganizationRegulatoryProgram":
                case "UpdateEnableDisableFlag":
                    {
                        //
                        //Authorize the correct Authority for this Org Reg Program
                        //

                        var targetOrgRegProgId = id[0];
                        if (currentPortalName.Equals("authority"))
                        {
                            if (currentOrgRegProgramId == targetOrgRegProgId)
                            {
                                retVal = true;
                            }
                            else
                            {
                                var targetOrgRegProgram = _dbContext.OrganizationRegulatoryPrograms
                                    .SingleOrDefault(orpu => orpu.OrganizationRegulatoryProgramId == targetOrgRegProgId);

                                //this will also handle scenarios where targetOrgRegProgId doesn't even exist
                                if (targetOrgRegProgram != null
                                    && targetOrgRegProgram.RegulatoryProgramId == currentRegulatoryProgramId
                                    && targetOrgRegProgram.RegulatorOrganizationId == currentOrganizationId)
                                {
                                    retVal = true;
                                }

                            }
                        }
                        else
                        {
                            //
                            //Authorize Industry Admins only
                            //

                            var currentUsersPermissionGroup = _dbContext.OrganizationRegulatoryProgramUsers
                                .Single(orpu => orpu.OrganizationRegulatoryProgramUserId == currentOrgRegProgUserId)
                                .PermissionGroup;

                            bool isAdmin = currentUsersPermissionGroup.Name.ToLower().StartsWith("admin")
                                            && currentUsersPermissionGroup.OrganizationRegulatoryProgramId == targetOrgRegProgId;

                            if (isAdmin)
                            {
                                //This is an authorized industry admin within the target organization regulatory program
                                retVal = true;
                            }

                        }

                    }

                    break;

                default:

                    throw new Exception($"ERROR: Unhandled API authorization attempt using name = '{apiName}'");


            }

            return retVal;
        }

        public IEnumerable<OrganizationRegulatoryProgramDto> GetUserOrganizations()
        {
            int userProfileId = Convert.ToInt32(_httpContext.Current.User.Identity.UserProfileId());
            return GetUserOrganizations(userProfileId);
        }

        /// <summary>
        /// Get organizations that a user can access to (IU portal, AU portal, content MGT portal)
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>Collection of organization</returns>
        public IEnumerable<OrganizationRegulatoryProgramDto> GetUserOrganizations(int userId)
        { 
            var orpUsers = _dbContext.OrganizationRegulatoryProgramUsers.ToList()
                .FindAll(u => u.UserProfileId == userId &&
                            u.IsRemoved == false &&
                            u.IsEnabled == true &&
                            u.IsRegistrationApproved &&
                            u.OrganizationRegulatoryProgram.IsEnabled &&
                            u.OrganizationRegulatoryProgram.IsRemoved == false);

            if (orpUsers == null)
            {
                return null;
            }
            return  orpUsers.Select(i =>
            {
                return _mapHelper.GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(i.OrganizationRegulatoryProgram);
            }); 
        }

        /// <summary>
        /// Return all the programs' regulators list for the user
        /// </summary>
        /// <param name="userId">The user Id</param>
        /// <returns>The organizationId </returns>
        public IEnumerable<AuthorityDto> GetUserRegulators(int userId, bool isIncludeRemoved = false)
        {
            try
            {
                var orpUsers = _dbContext.OrganizationRegulatoryProgramUsers.ToList()
                    .FindAll(u => u.UserProfileId == userId &&
                               // u.IsEnabled == true &&
                                //u.IsRegistrationApproved &&
                                //u.OrganizationRegulatoryProgram.IsEnabled &&
                                u.OrganizationRegulatoryProgram.IsRemoved == false);

                if (!isIncludeRemoved)
                {
                    orpUsers = orpUsers.FindAll(u => !u.IsRemoved);
                }

                var orgs = new List<AuthorityDto>();
                foreach (var orpUser in orpUsers)
                {
                    OrganizationRegulatoryProgram authority;
                    var thisOrgRegProgram = orpUser.OrganizationRegulatoryProgram;
                    if (thisOrgRegProgram.RegulatorOrganization != null)
                        authority = _dbContext.OrganizationRegulatoryPrograms
                            .Single(o => o.OrganizationId == thisOrgRegProgram.RegulatorOrganization.OrganizationId
                            && o.RegulatoryProgramId == thisOrgRegProgram.RegulatoryProgramId);
                    else
                        authority = thisOrgRegProgram;

                    if (authority?.Organization.OrganizationType.Name == "Authority")
                    {
                        //Check for duplicates before adding
                        if (!orgs.Any(i => i.OrganizationRegulatoryProgramId == authority.OrganizationRegulatoryProgramId))
                        {
                            AuthorityDto authorityDto = _mapHelper.GetAuthorityDtoFromOrganization(authority.Organization);
                            authorityDto.RegulatoryProgramId = authority.RegulatoryProgramId;
                            authorityDto.OrganizationRegulatoryProgramId = authority.OrganizationRegulatoryProgramId;
                            authorityDto.EmailContactInfoName = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoName);
                            authorityDto.EmailContactInfoEmailAddress = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoEmailAddress);
                            authorityDto.EmailContactInfoPhone = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoPhone);
                            orgs.Add(authorityDto);

                        }
                    }
                    else
                        throw new Exception(string.Format("ERROR: Organization {0} in Program {1} does not have a regulator and is not itself of type 'Authority'. ",
                            authority.OrganizationId, orpUser.OrganizationRegulatoryProgram.RegulatoryProgramId));

                }

                return orgs;
            }
            catch (DbEntityValidationException ex)
            {
                HandleEntityException(ex);
            }

            return null;
        }

        public string GetUserAuthorityListForEmailContent(int userProfileId)
        {
            var authorities = GetUserRegulators(userProfileId);
            //Find all possible authorities
            var authorityList = "";
            var newLine = "";
            foreach (var authority in authorities)
            {
                authorityList += newLine + authority.EmailContactInfoName +
                    " at " + authority.EmailContactInfoEmailAddress +
                    " or " + authority.EmailContactInfoPhone;
                //if (!String.IsNullOrEmpty(authority.PhoneExt))
                //    authorityList += " ext." + authority.PhoneExt;
                newLine = Environment.NewLine;
            }

            return authorityList;
        }

        /// <summary>
        /// Get the organization by organization id
        /// </summary>
        /// <param name="organizationId">Organization id</param>
        /// <returns>Collection of organization</returns>
        public OrganizationDto GetOrganization(int organizationId)
        {
            OrganizationDto returnDto = null;
            try
            {
                var foundOrg = _dbContext.Organizations.Single(o => o.OrganizationId == organizationId);
                returnDto = _mapHelper.GetOrganizationDtoFromOrganization(foundOrg);
                JurisdictionDto jurisdiction = _jurisdictionService.GetJurisdictionById(foundOrg.JurisdictionId);

                returnDto.State = jurisdiction?.Code ?? "";
            }
            catch (DbEntityValidationException ex)
            {
                HandleEntityException(ex);
            }

            return returnDto;
        }

        /// When enabling, we need to check the parent (RegulatorOrganizationId)
        /// to see if there are any available licenses left
        ///
        /// Otherwise throw exception
        public EnableOrganizationResultDto UpdateEnableDisableFlag(int orgRegProgId, bool isEnabled, bool isAuthorizationRequired = false)
        {
            if (isAuthorizationRequired && !CanUserExecuteApi(id:orgRegProgId))
            {
                throw new UnauthorizedAccessException();
            }

            var orgRegProg = _dbContext.OrganizationRegulatoryPrograms
                    .Include(path: "RegulatoryProgram")
                    .Include(path: "Organization")
                    .Single(o => o.OrganizationRegulatoryProgramId == orgRegProgId);

            bool isAuthority = orgRegProg.RegulatorOrganizationId == null;
            if (isEnabled && !isAuthority)
            {
                //check if violates max industries allowed for authority
                var authority = _dbContext.OrganizationRegulatoryPrograms
                    .Single(o => o.OrganizationId == orgRegProg.RegulatorOrganizationId
                    && o.RegulatoryProgramId == orgRegProg.RegulatoryProgramId);

                var remainingLicenses = GetRemainingIndustryLicenseCount(authority.OrganizationRegulatoryProgramId);
                if (remainingLicenses < 1)
                    return new EnableOrganizationResultDto() { IsSuccess = false, FailureReason = EnableOrganizationFailureReason.TooManyIndustriesForAuthority };

                //Check child organization doesn't have more user's than "UserPerIndustryMaxCount" setting of parent
                var remainingUserCount = GetRemainingUserLicenseCount(orgRegProgId);
                if (remainingUserCount < 1)
                    return new EnableOrganizationResultDto() { IsSuccess = false, FailureReason = EnableOrganizationFailureReason.TooManyUsersForThisIndustry };

            }

            orgRegProg.IsEnabled = isEnabled;
            _dbContext.SaveChanges();

            return new EnableOrganizationResultDto() { IsSuccess = true };
        } 

        public OrganizationRegulatoryProgramDto GetOrganizationRegulatoryProgram(int orgRegProgId, bool isAuthorizationRequired = false)
        {
            if (isAuthorizationRequired && !CanUserExecuteApi(id:orgRegProgId))
            {
                throw new UnauthorizedAccessException();
            }

            var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgId);
            OrganizationRegulatoryProgramDto dto = _mapHelper.GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(orgRegProgram);
            var signatoryUserCount = _dbContext.OrganizationRegulatoryProgramUsers
                .Count(o => o.OrganizationRegulatoryProgramId == orgRegProgram.OrganizationRegulatoryProgramId
                && o.IsSignatory == true);
            dto.HasSignatory = signatoryUserCount > 0;
            var adminUserCount = _dbContext.OrganizationRegulatoryProgramUsers.Include("PermissionGroup")
                .Count(o => o.OrganizationRegulatoryProgramId == orgRegProgram.OrganizationRegulatoryProgramId
                && o.PermissionGroup.Name == UserRole.Administrator.ToString()
                && o.IsRegistrationApproved == true
                && o.IsRegistrationDenied == false
                && o.IsEnabled == true
                && o.IsRemoved == false);
            dto.HasActiveAdmin = adminUserCount > 0;
            dto.OrganizationDto.State = _jurisdictionService.GetJurisdictionById(orgRegProgram.Organization.JurisdictionId)?.Code ?? "";

            var lastReportPackageSubmitted = _dbContext.ReportPackages
                .Where(rp => rp.OrganizationRegulatoryProgramId == orgRegProgId
                    && rp.SubmissionDateTimeUtc != null)
                .OrderByDescending(rp => rp.SubmissionDateTimeUtc)
                .FirstOrDefault();
            if (lastReportPackageSubmitted != null)
            {
                dto.LastSubmissionDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingSettingForThisOrg(lastReportPackageSubmitted.SubmissionDateTimeUtc.Value.UtcDateTime
                        , orgRegProgId);
            }

            return dto;
        }

        public List<OrganizationRegulatoryProgramDto> GetChildOrganizationRegulatoryPrograms(int orgRegProgId, string searchString = null)
        {
            try
            {
                var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgId);
                var childOrgRegProgs = _dbContext.OrganizationRegulatoryPrograms.Where(o => o.RegulatorOrganizationId == orgRegProgram.OrganizationId
                    && o.RegulatoryProgramId == orgRegProgram.RegulatoryProgramId);

                if (childOrgRegProgs != null && !string.IsNullOrEmpty(searchString))
                {
                    childOrgRegProgs = childOrgRegProgs.Where(x =>
                                                               x.ReferenceNumber.Contains(searchString)
                                                            || x.Organization.Name.Contains(searchString) 
                                                            || x.Organization.AddressLine1.Contains(searchString)
                                                            || x.Organization.AddressLine2.Contains(searchString)
                                                            || x.Organization.CityName.Contains(searchString)
                                                            || (x.Organization.Jurisdiction != null && x.Organization.Jurisdiction.Code.Contains(searchString))
                                                            || x.Organization.ZipCode.Contains(searchString));
                }

                if (childOrgRegProgs != null)
                {
                    var dtoList = new List<OrganizationRegulatoryProgramDto>();
                    foreach (var orgRegProg in childOrgRegProgs.ToList())
                    {
                        OrganizationRegulatoryProgramDto dto = _mapHelper.GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(orgRegProg);
                        dto.HasSignatory = _dbContext.OrganizationRegulatoryProgramUsers
                                                     .Count(o => o.OrganizationRegulatoryProgramId == orgRegProg.OrganizationRegulatoryProgramId && o.IsSignatory)
                                           > 0;
                        dto.HasActiveAdmin = _dbContext.OrganizationRegulatoryProgramUsers.Include("PermissionGroup")
                                                       .Count(o => o.OrganizationRegulatoryProgramId == orgRegProgram.OrganizationRegulatoryProgramId
                                                                   && o.IsRegistrationApproved
                                                                   && o.IsEnabled
                                                                   && o.PermissionGroup.Name == "Administrator"
                                                             )
                                             > 0;
                        dto.OrganizationDto.State = _jurisdictionService.GetJurisdictionById(orgRegProgram.Organization.JurisdictionId)?.Code ?? "";
                        dtoList.Add(dto);

                    }
                    return dtoList;
                }
                else
                {
                    return null;
                }
            }
            catch (DbEntityValidationException ex)
            {
                List<RuleViolation> validationIssues = new List<RuleViolation>();
                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;

                    foreach (DbValidationError subItem in item.ValidationErrors)
                    {
                        string message = string.Format("Error '{0}' occurred in {1} at {2}", subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                        validationIssues.Add(new RuleViolation(string.Empty, null, message));

                    }
                }
                //_logger.Info("???");
                throw new RuleViolationException("Validation errors", validationIssues);
            }

        }

        private void HandleEntityException(DbEntityValidationException ex)
        {
            List<RuleViolation> validationIssues = new List<RuleViolation>();
            foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
            {
                DbEntityEntry entry = item.Entry;
                string entityTypeName = entry.Entity.GetType().Name;

                foreach (DbValidationError subItem in item.ValidationErrors)
                {
                    string message = string.Format("Error '{0}' occurred in {1} at {2}", subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                    validationIssues.Add(new RuleViolation(string.Empty, null, message));

                }
            }
            //_logger.Info("???");
            throw new RuleViolationException("Validation errors", validationIssues);
        }
                
        public int GetRemainingUserLicenseCount(int orgRegProgramId)
        {
            int maxCount;
            
            //Authority or Industry?
            var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms
                .Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId);
            bool isForAuthority = !orgRegProgram.RegulatorOrganizationId.HasValue;

            if (isForAuthority)
            {
                maxCount = Convert.ToInt32(_settingService.GetOrgRegProgramSettingValue(orgRegProgramId, SettingType.AuthorityUserLicenseTotalCount));
            }
            else
            {
                //Setting will be at the Authority of this Industry
                var thisIndustry = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId);
                var authority = _dbContext.OrganizationRegulatoryPrograms
                                          .Single(o => o.OrganizationId == thisIndustry.RegulatorOrganizationId && o.RegulatoryProgramId == thisIndustry.RegulatoryProgramId);

                maxCount = Convert.ToInt32(_settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.UserPerIndustryMaxCount));
            }
            var currentProgramUserCount = _dbContext.OrganizationRegulatoryProgramUsers
                                                    .Count(u => u.OrganizationRegulatoryProgramId == orgRegProgramId
                                                    && !u.IsRemoved && u.IsRegistrationApproved && u.IsEnabled);

            return maxCount - currentProgramUserCount;

        }

        public int GetRemainingIndustryLicenseCount(int orgRegProgramId)
        {
            var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId);
            var currentChildIndustryCount = _dbContext.OrganizationRegulatoryPrograms
                .Count(u => u.RegulatorOrganizationId == orgRegProgram.OrganizationId
                && u.RegulatoryProgramId == orgRegProgram.RegulatoryProgramId
                && u.IsRemoved != true && u.IsEnabled == true);

            int maxCount = Convert.ToInt32(_settingService.GetOrgRegProgramSettingValue(orgRegProgramId, SettingType.IndustryLicenseTotalCount));
            var remaining = maxCount - currentChildIndustryCount;

            //Handle at caller
            //if (remaining < 0)
            //    throw new Exception(string.Format("ERROR: Remaining industry license count is a negative number (={0}) for Org Reg Program={1}", remaining, orgRegProgramId));

            return remaining;

        }

        public int GetCurrentUserLicenseCount(int orgRegProgramId)
        {
            var currentProgramUserCount = _dbContext.OrganizationRegulatoryProgramUsers
                                                    .Count(u => u.OrganizationRegulatoryProgramId == orgRegProgramId
                                                    && !u.IsRemoved && u.IsRegistrationApproved && u.IsEnabled);
            return currentProgramUserCount;

        }

        public int GetCurrentIndustryLicenseCount(int orgRegProgramId)
        {
            var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId);
            var currentChildIndustryCount = _dbContext.OrganizationRegulatoryPrograms
                .Count(u => u.RegulatorOrganizationId == orgRegProgram.OrganizationId
                && u.RegulatoryProgramId == orgRegProgram.RegulatoryProgramId
                && u.IsRemoved != true && u.IsEnabled == true);

            return currentChildIndustryCount;
        }


        public OrganizationRegulatoryProgramDto GetAuthority(int orgRegProgramId)
        {
            var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Include("Organization.Jurisdiction")
                .Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId);
            OrganizationRegulatoryProgram authorityRegProgram;
            if (orgRegProgram.RegulatorOrganization != null)
            {
                authorityRegProgram = _dbContext.OrganizationRegulatoryPrograms.Include("Organization")
                                      .Single(o => o.OrganizationId == orgRegProgram.RegulatorOrganization.OrganizationId
                                                   && o.RegulatoryProgramId == orgRegProgram.RegulatoryProgramId);

            }
            else
            {
                authorityRegProgram = orgRegProgram;
            }

            var authorityDto = _mapHelper.GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(authorityRegProgram);
            return authorityDto;
        }

    }
}
 