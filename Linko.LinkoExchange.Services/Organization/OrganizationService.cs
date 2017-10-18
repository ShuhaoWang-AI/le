using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Extensions;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Base;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Jurisdiction;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;

namespace Linko.LinkoExchange.Services.Organization
{
    public class OrganizationService : BaseService, IOrganizationService
    {
        #region fields

        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContext;
        private readonly IJurisdictionService _jurisdictionService;
        private readonly IMapHelper _mapHelper;
        private readonly ISettingService _settingService;
        private readonly ITimeZoneService _timeZoneService;

        #endregion

        #region constructors and destructor

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

        #endregion

        #region interface implementations

        public override bool CanUserExecuteApi([CallerMemberName] string apiName = "", params int[] id)
        {
            var retVal = false;

            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var currentOrgRegProgUserId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramUserId));
            var currentOrganizationId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationId));
            var currentRegulatoryProgramId = _dbContext.OrganizationRegulatoryPrograms.Single(orp => orp.OrganizationRegulatoryProgramId == currentOrgRegProgramId)
                                                       .RegulatoryProgramId;
            var currentPortalName = _httpContext.GetClaimValue(claimType:CacheKey.PortalName);
            currentPortalName = string.IsNullOrWhiteSpace(value:currentPortalName) ? "" : currentPortalName.Trim().ToLower();

            switch (apiName)
            {
                case "GetOrganizationRegulatoryProgram":
                case "UpdateEnableDisableFlag":
                {
                    //
                    //Authorize the correct Authority for this Org Reg Program
                    //

                    var targetOrgRegProgId = id[0];
                    if (currentPortalName.Equals(value:"authority"))
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
                        //Authorize Industry Admins only

                        var currentUsersPermissionGroup = _dbContext.OrganizationRegulatoryProgramUsers
                                                                    .Single(orpu => orpu.OrganizationRegulatoryProgramUserId == currentOrgRegProgUserId)
                                                                    .PermissionGroup;

                        var isAdmin = currentUsersPermissionGroup.Name.ToLower().StartsWith(value:"admin")
                                      && currentUsersPermissionGroup.OrganizationRegulatoryProgramId == targetOrgRegProgId;

                        if (isAdmin)
                        {
                            //This is an authorized industry admin within the target organization regulatory program
                            retVal = true;
                        }
                    }
                }

                    break;

                default: throw new Exception(message:$"ERROR: Unhandled API authorization attempt using name = '{apiName}'");
            }

            return retVal;
        }

        public IEnumerable<OrganizationRegulatoryProgramDto> GetUserOrganizations()
        {
            var userProfileId = Convert.ToInt32(value:_httpContext.Current.User.Identity.UserProfileId());
            return GetUserOrganizations(userId:userProfileId);
        }

        /// <summary>
        ///     Get organizations that a user can access to (IU portal, AU portal, content MGT portal)
        /// </summary>
        /// <param name="userId"> User id </param>
        /// <returns> Collection of organization </returns>
        public IEnumerable<OrganizationRegulatoryProgramDto> GetUserOrganizations(int userId)
        {
            var orpUsers = _dbContext.OrganizationRegulatoryProgramUsers.ToList()
                                     .FindAll(u => u.UserProfileId == userId
                                                   && u.IsRemoved == false
                                                   && u.IsEnabled
                                                   && u.IsRegistrationApproved
                                                   && u.OrganizationRegulatoryProgram.IsEnabled
                                                   && u.OrganizationRegulatoryProgram.IsRemoved == false);
            
            return orpUsers.Select(i => _mapHelper.GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(orgRegProgram:i.OrganizationRegulatoryProgram));
        }

        /// <summary>
        ///     Return all the programs' regulators list for the user
        /// </summary>
        /// <param name="userId"> The user Id </param>
        /// <param name="isIncludeRemoved"> </param>
        /// <returns> The organizationId </returns>
        public IEnumerable<AuthorityDto> GetUserRegulators(int userId, bool isIncludeRemoved = false)
        {
            var orpUsers = _dbContext.OrganizationRegulatoryProgramUsers.ToList()
                                     .FindAll(u => u.UserProfileId == userId
                                                   &&

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
                {
                    authority = _dbContext.OrganizationRegulatoryPrograms
                                          .Single(o => o.OrganizationId == thisOrgRegProgram.RegulatorOrganization.OrganizationId
                                                       && o.RegulatoryProgramId == thisOrgRegProgram.RegulatoryProgramId);
                }
                else
                {
                    authority = thisOrgRegProgram;
                }

                if (authority?.Organization.OrganizationType.Name.ToLower() == OrganizationTypeName.Authority.ToString().ToLower())
                {
                    //Check for duplicates before adding
                    if (!orgs.Any(i => i.OrganizationRegulatoryProgramId == authority.OrganizationRegulatoryProgramId))
                    {
                        var authorityDto = _mapHelper.GetAuthorityDtoFromOrganization(organization:authority.Organization);
                        authorityDto.RegulatoryProgramId = authority.RegulatoryProgramId;
                        authorityDto.OrganizationRegulatoryProgramId = authority.OrganizationRegulatoryProgramId;
                        authorityDto.EmailContactInfoName =
                            _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authority.OrganizationRegulatoryProgramId,
                                                                         settingType:SettingType.EmailContactInfoName);
                        authorityDto.EmailContactInfoEmailAddress =
                            _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authority.OrganizationRegulatoryProgramId,
                                                                         settingType:SettingType.EmailContactInfoEmailAddress);
                        authorityDto.EmailContactInfoPhone =
                            _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authority.OrganizationRegulatoryProgramId,
                                                                         settingType:SettingType.EmailContactInfoPhone);
                        orgs.Add(item:authorityDto);
                    }
                }
                else
                {
                    throw new Exception(message:string.Format(format:"ERROR: Organization {0} in Program {1} does not have a regulator and is not itself of type 'Authority'. ",
                                                              arg0:authority.OrganizationId, arg1:orpUser.OrganizationRegulatoryProgram.RegulatoryProgramId));
                }
            }

            return orgs;
        }

        public string GetUserAuthorityListForEmailContent(int userProfileId)
        {
            var authorities = GetUserRegulators(userId:userProfileId).ToList();

            //Find all possible authorities
            var authorityList = "";
            var newLine = Environment.NewLine;
            var leadingStr = "    "; 

            foreach (var authority in authorities) {
                authorityList += newLine + leadingStr + authority.EmailContactInfoName + " at " + authority.EmailContactInfoEmailAddress + " or " + authority.EmailContactInfoPhone;
            }

            return authorityList;
        }

        /// <summary>
        ///     Get the organization by organization id
        /// </summary>
        /// <param name="organizationId"> Organization id </param>
        /// <returns> Collection of organization </returns>
        public OrganizationDto GetOrganization(int organizationId)
        {
            var foundOrg = _dbContext.Organizations.Single(o => o.OrganizationId == organizationId);
            var returnDto = _mapHelper.GetOrganizationDtoFromOrganization(organization:foundOrg);
            var jurisdiction = _jurisdictionService.GetJurisdictionById(jurisdictionId:foundOrg.JurisdictionId);

            returnDto.State = jurisdiction?.Code ?? "";

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
                                       .Include(path:"RegulatoryProgram")
                                       .Include(path:"Organization")
                                       .Single(o => o.OrganizationRegulatoryProgramId == orgRegProgId);

            var isAuthority = orgRegProg.RegulatorOrganizationId == null;
            if (isEnabled && !isAuthority)
            {
                //check if violates max industries allowed for authority
                var authority = _dbContext.OrganizationRegulatoryPrograms
                                          .Single(o => o.OrganizationId == orgRegProg.RegulatorOrganizationId
                                                       && o.RegulatoryProgramId == orgRegProg.RegulatoryProgramId);

                var remainingLicenses = GetRemainingIndustryLicenseCount(orgRegProgramId:authority.OrganizationRegulatoryProgramId);
                if (remainingLicenses < 1)
                {
                    return new EnableOrganizationResultDto {IsSuccess = false, FailureReason = EnableOrganizationFailureReason.TooManyIndustriesForAuthority};
                }

                //Check child organization doesn't have more user's than "UserPerIndustryMaxCount" setting of parent
                var remainingUserCount = GetRemainingUserLicenseCount(orgRegProgramId:orgRegProgId);
                if (remainingUserCount < 1)
                {
                    return new EnableOrganizationResultDto {IsSuccess = false, FailureReason = EnableOrganizationFailureReason.TooManyUsersForThisIndustry};
                }
            }

            orgRegProg.IsEnabled = isEnabled;
            _dbContext.SaveChanges();

            return new EnableOrganizationResultDto {IsSuccess = true};
        }

        public OrganizationRegulatoryProgramDto GetOrganizationRegulatoryProgram(int orgRegProgId, bool isAuthorizationRequired = false)
        {
            if (isAuthorizationRequired && !CanUserExecuteApi(id:orgRegProgId))
            {
                throw new UnauthorizedAccessException();
            }

            var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgId);
            var dto = _mapHelper.GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(orgRegProgram:orgRegProgram);
            var signatoryUserCount = _dbContext.OrganizationRegulatoryProgramUsers
                                               .Count(o => o.OrganizationRegulatoryProgramId == orgRegProgram.OrganizationRegulatoryProgramId
                                                           && o.IsSignatory);
            dto.HasSignatory = signatoryUserCount > 0;
            var adminUserCount = _dbContext.OrganizationRegulatoryProgramUsers.Include(path:"PermissionGroup")
                                           .Count(o => o.OrganizationRegulatoryProgramId == orgRegProgram.OrganizationRegulatoryProgramId
                                                       && o.PermissionGroup.Name == UserRole.Administrator.ToString()
                                                       && o.IsRegistrationApproved
                                                       && o.IsRegistrationDenied == false
                                                       && o.IsEnabled
                                                       && o.IsRemoved == false);
            dto.HasActiveAdmin = adminUserCount > 0;
            dto.OrganizationDto.State = _jurisdictionService.GetJurisdictionById(jurisdictionId:orgRegProgram.Organization.JurisdictionId)?.Code ?? "";

            var lastReportPackageSubmitted = _dbContext.ReportPackages
                                                       .Where(rp => rp.OrganizationRegulatoryProgramId == orgRegProgId
                                                                    && rp.SubmissionDateTimeUtc != null)
                                                       .OrderByDescending(rp => rp.SubmissionDateTimeUtc)
                                                       .FirstOrDefault();
            if (lastReportPackageSubmitted != null)
            {
                dto.LastSubmissionDateTimeLocal =
                    _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:lastReportPackageSubmitted.SubmissionDateTimeUtc.Value.UtcDateTime,
                                                                                orgRegProgramId:orgRegProgId);
            }

            return dto;
        }

        public List<OrganizationRegulatoryProgramDto> GetChildOrganizationRegulatoryPrograms(int orgRegProgId, string searchString = null)
        {
            var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgId);
            var childOrgRegProgs = _dbContext.OrganizationRegulatoryPrograms.Where(o => o.RegulatorOrganizationId == orgRegProgram.OrganizationId
                                                                                        && o.RegulatoryProgramId == orgRegProgram.RegulatoryProgramId);

            if (!string.IsNullOrEmpty(value:searchString))
            {
                childOrgRegProgs = childOrgRegProgs.Where(x =>

                                                              // ReSharper disable once ArgumentsStyleNamedExpression
                                                                  x.ReferenceNumber.Contains(searchString)

                                                                  // ReSharper disable once ArgumentsStyleNamedExpression
                                                                  || x.Organization.Name.Contains(searchString)

                                                                  // ReSharper disable once ArgumentsStyleNamedExpression
                                                                  || x.Organization.AddressLine1.Contains(searchString)

                                                                  // ReSharper disable once ArgumentsStyleNamedExpression
                                                                  || x.Organization.AddressLine2.Contains(searchString)

                                                                  // ReSharper disable once ArgumentsStyleNamedExpression
                                                                  || x.Organization.CityName.Contains(searchString)

                                                                  // ReSharper disable once ArgumentsStyleNamedExpression
                                                                  || x.Organization.Jurisdiction != null && x.Organization.Jurisdiction.Code.Contains(searchString)

                                                                  // ReSharper disable once ArgumentsStyleNamedExpression
                                                                  || x.Organization.ZipCode.Contains(searchString));
            }

            var dtos = new List<OrganizationRegulatoryProgramDto>();
            foreach (var childOrgRegProg in childOrgRegProgs.ToList())
            {
                var dto = _mapHelper.GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(orgRegProgram:childOrgRegProg);
                dto.HasSignatory = _dbContext.OrganizationRegulatoryProgramUsers
                                             .Count(o => o.OrganizationRegulatoryProgramId == childOrgRegProg.OrganizationRegulatoryProgramId && o.IsSignatory)
                                   > 0;
                dto.HasActiveAdmin = _dbContext.OrganizationRegulatoryProgramUsers.Include(path:"PermissionGroup")
                                               .Count(o => o.OrganizationRegulatoryProgramId == childOrgRegProg.OrganizationRegulatoryProgramId
                                                           && o.IsRegistrationApproved
                                                           && o.IsEnabled
                                                           && o.PermissionGroup.Name == "Administrator"
                                                     )
                                     > 0;
                dtos.Add(item:dto);
            }

            return dtos;
        }

        public int GetRemainingUserLicenseCount(int orgRegProgramId)
        {
            int maxCount;

            //Authority or Industry?
            var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms
                                          .Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId);
            var isForAuthority = !orgRegProgram.RegulatorOrganizationId.HasValue;

            if (isForAuthority)
            {
                maxCount = Convert.ToInt32(value:_settingService.GetOrgRegProgramSettingValue(orgRegProgramId:orgRegProgramId,
                                                                                              settingType:SettingType.AuthorityUserLicenseTotalCount));
            }
            else
            {
                //Setting will be at the Authority of this Industry
                var thisIndustry = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId);
                var authority = _dbContext.OrganizationRegulatoryPrograms
                                          .Single(o => o.OrganizationId == thisIndustry.RegulatorOrganizationId && o.RegulatoryProgramId == thisIndustry.RegulatoryProgramId);

                maxCount = Convert.ToInt32(value:_settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authority.OrganizationRegulatoryProgramId,
                                                                                              settingType:SettingType.UserPerIndustryMaxCount));
            }
            var currentProgramUserCount = GetCurrentUserLicenseCount(orgRegProgramId:orgRegProgramId);

            return maxCount - currentProgramUserCount;
        }

        public int GetRemainingIndustryLicenseCount(int orgRegProgramId)
        {
            var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId);
            var currentChildIndustryCount = _dbContext.OrganizationRegulatoryPrograms
                                                      .Count(u => u.RegulatorOrganizationId == orgRegProgram.OrganizationId
                                                                  && u.RegulatoryProgramId == orgRegProgram.RegulatoryProgramId
                                                                  && u.IsRemoved != true
                                                                  && u.IsEnabled);

            var maxCount = Convert.ToInt32(value:_settingService.GetOrgRegProgramSettingValue(orgRegProgramId:orgRegProgramId, settingType:SettingType.IndustryLicenseTotalCount));
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
                                                                && !u.IsRemoved
                                                                && u.IsRegistrationApproved
                                                                && u.IsEnabled);
            return currentProgramUserCount;
        }

        public int GetCurrentIndustryLicenseCount(int orgRegProgramId)
        {
            var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId);
            var currentChildIndustryCount = _dbContext.OrganizationRegulatoryPrograms
                                                      .Count(u => u.RegulatorOrganizationId == orgRegProgram.OrganizationId
                                                                  && u.RegulatoryProgramId == orgRegProgram.RegulatoryProgramId
                                                                  && u.IsRemoved != true
                                                                  && u.IsEnabled);

            return currentChildIndustryCount;
        }

        public OrganizationRegulatoryProgramDto GetAuthority(int orgRegProgramId)
        {
            var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Include(path:"Organization.Jurisdiction")
                                          .Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId);
            OrganizationRegulatoryProgram authorityRegProgram;
            if (orgRegProgram.RegulatorOrganization != null)
            {
                authorityRegProgram = _dbContext.OrganizationRegulatoryPrograms.Include(path:"Organization")
                                                .Single(o => o.OrganizationId == orgRegProgram.RegulatorOrganization.OrganizationId
                                                             && o.RegulatoryProgramId == orgRegProgram.RegulatoryProgramId);
            }
            else
            {
                authorityRegProgram = orgRegProgram;
            }

            var authorityDto = _mapHelper.GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(orgRegProgram:authorityRegProgram);
            return authorityDto;
        }

        #endregion
    }
}