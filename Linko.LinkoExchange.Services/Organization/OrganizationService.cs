using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using AutoMapper;
using Linko.LinkoExchange.Core.Common;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Extensions;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.Jurisdiction;
using System.Data.Entity;

namespace Linko.LinkoExchange.Services.Organization
{
    public class OrganizationService : IOrganizationService
    {
        private readonly LinkoExchangeContext _dbContext;
        //private readonly IAuditLogEntry _logger;
        private readonly IMapper _mapper;
        private readonly ISettingService _settingService;
        private readonly IHttpContextService _httpContext;
        private readonly IJurisdictionService _jurisdictionService;

        public OrganizationService(LinkoExchangeContext dbContext, IMapper mapper,
            ISettingService settingService, IHttpContextService httpContext, IJurisdictionService jurisdictionService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _settingService = settingService;
            _httpContext = httpContext;
            _jurisdictionService = jurisdictionService;
        }

        public IEnumerable<OrganizationDto> GetUserOrganizationsByOrgRegProgUserId(int orgRegProgUserId)
        {
            var orgDtoList = new List<OrganizationDto>();
            var orgRegProgramId = _dbContext.OrganizationRegulatoryProgramUsers.Single(o => o.OrganizationRegulatoryProgramUserId == orgRegProgUserId).OrganizationRegulatoryProgramId;
            var regProgramId = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId).RegulatoryProgramId;
            var orgList = _dbContext.OrganizationRegulatoryPrograms.Include(o => o.Organization.OrganizationType).Where(o => o.RegulatoryProgramId == regProgramId);
            if (orgList != null)
            {
                foreach (var org in orgList)
                {
                    orgDtoList.Add(_mapper.Map<Core.Domain.Organization, OrganizationDto>(org.Organization));
                }
            }
            return orgDtoList;
        }

        public IEnumerable<OrganizationRegulatoryProgramDto> GetUserOrganizations()
        {
            int userProfileId = Convert.ToInt32(_httpContext.Current().User.Identity.UserProfileId());
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
                return _mapper.Map<OrganizationRegulatoryProgramDto>(i.OrganizationRegulatoryProgram);
            }); 
        }

        /// <summary>
        /// Return all the programs' regulators list for the user
        /// </summary>
        /// <param name="userId">The user Id</param>
        /// <returns>The organizationId </returns>
        public IEnumerable<AuthorityDto> GetUserRegulators(int userId)
        {
            try
            {
                var orpUsers = _dbContext.OrganizationRegulatoryProgramUsers.ToList()
                    .FindAll(u => u.UserProfileId == userId &&
                                u.IsRemoved == false &&
                               // u.IsEnabled == true &&
                                u.IsRegistrationApproved &&
                                u.OrganizationRegulatoryProgram.IsEnabled &&
                                u.OrganizationRegulatoryProgram.IsRemoved == false);

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
                            AuthorityDto authorityDto = _mapper.Map<Core.Domain.Organization, AuthorityDto>(authority.Organization);
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
                returnDto = _mapper.Map<Core.Domain.Organization, OrganizationDto>(foundOrg);
                var jurisdiction = _jurisdictionService.GetJurisdictionById(foundOrg.JurisdictionId);
                returnDto.State = jurisdiction == null ? "" : jurisdiction.Name;
            }
            catch (DbEntityValidationException ex)
            {
                HandleEntityException(ex);
            }

            return returnDto;
        }

        public void UpdateOrganization(OrganizationDto organization)
        {
            try
            {
                var foundOrg = _dbContext.Organizations.Single(o => o.OrganizationId == organization.OrganizationId);
                foundOrg = _mapper.Map<OrganizationDto, Core.Domain.Organization>(organization);
                _dbContext.SaveChanges();
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

        /// When enabling, we need to check the parent (RegulatorOrganizationId)
        /// to see if there are any available licenses left
        ///
        /// Otherwise throw exception
        public EnableOrganizationResultDto UpdateEnableDisableFlag(int orgRegProgId, bool isEnabled)
        {
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
                var remainingUserCount = GetRemainingUserLicenseCount(orgRegProgId, false);
                if (remainingUserCount < 1)
                    return new EnableOrganizationResultDto() { IsSuccess = false, FailureReason = EnableOrganizationFailureReason.TooManyUsersForThisIndustry };

            }

            orgRegProg.IsEnabled = isEnabled;
            _dbContext.SaveChanges();

            return new EnableOrganizationResultDto() { IsSuccess = true };
        } 

        public OrganizationRegulatoryProgramDto GetOrganizationRegulatoryProgram(int orgRegProgId)
        {
            var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgId);
            OrganizationRegulatoryProgramDto dto = _mapper.Map<OrganizationRegulatoryProgram, OrganizationRegulatoryProgramDto>(orgRegProgram);
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
            dto.HasAdmin = adminUserCount > 0;
            dto.OrganizationDto.State = _jurisdictionService.GetJurisdictionById(orgRegProgram.Organization.JurisdictionId).Code;

            return dto;
        }

        public List<OrganizationRegulatoryProgramDto> GetChildOrganizationRegulatoryPrograms(int orgRegProgId, string searchString = null)
        {
            try
            {
                var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgId);
                var childOrgRegProgs = _dbContext.OrganizationRegulatoryPrograms.Where(o => o.RegulatorOrganizationId == orgRegProgram.OrganizationId
                    && o.RegulatoryProgramId == orgRegProgram.RegulatoryProgramId);

                if (childOrgRegProgs != null && !String.IsNullOrEmpty(searchString))
                {
                    childOrgRegProgs = childOrgRegProgs.Where(x =>
                                                               x.Organization.OrganizationId.ToString().Contains(searchString)
                                                            || x.Organization.Name.Contains(searchString) 
                                                            || x.Organization.AddressLine1.Contains(searchString)
                                                            || x.Organization.AddressLine2.Contains(searchString)
                                                            || x.Organization.CityName.Contains(searchString)
                                                            || x.Organization.Jurisdiction.Code.Contains(searchString)
                                                            || x.Organization.ZipCode.Contains(searchString));
                }

                if (childOrgRegProgs != null)
                {
                    var dtoList = new List<OrganizationRegulatoryProgramDto>();
                    foreach (var orgRegProg in childOrgRegProgs.ToList())
                    {
                        OrganizationRegulatoryProgramDto dto = _mapper.Map<OrganizationRegulatoryProgram, OrganizationRegulatoryProgramDto>(orgRegProg);
                        dto.HasSignatory = _dbContext.OrganizationRegulatoryProgramUsers
                            .Count(o => o.OrganizationRegulatoryProgramId == orgRegProg.OrganizationRegulatoryProgramId
                            && o.IsSignatory == true) > 0;
                        dto.HasAdmin = _dbContext.OrganizationRegulatoryProgramUsers.Include("PermissionGroup")
                            .Count(o => o.OrganizationRegulatoryProgramId == orgRegProgram.OrganizationRegulatoryProgramId
                            && o.PermissionGroup.Name == "Administrator") > 0;
                        dto.OrganizationDto.State = _jurisdictionService.GetJurisdictionById(orgRegProg.Organization.JurisdictionId).Code;
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

        public void AddChildOrganization(int parentRegOrdId, OrganizationDto childOrganization)
        {
            //ASSUMPTION: Organization record does not already exist
            try
            {
                var newOrg = _mapper.Map<OrganizationDto, Core.Domain.Organization>(childOrganization);

                var newOrgRegProg = _dbContext.OrganizationRegulatoryPrograms.Create();
                newOrgRegProg.Organization = newOrg;
                newOrgRegProg.RegulatorOrganizationId = parentRegOrdId;
                _dbContext.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                HandleEntityException(ex);
            }
            catch (Exception e)
            {
                var linkoException = new LinkoExchangeException();
                linkoException.ErrorType = LinkoExchangeError.OrganizationSetting;
                linkoException.Errors = new List<string> { e.Message };
                throw linkoException;
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
                
        public int GetRemainingUserLicenseCount(int orgRegProgramId, bool isForAuthority)
        {
            int maxCount;

            if (isForAuthority)
                maxCount = Convert.ToInt32(_settingService.GetOrgRegProgramSettingValue(orgRegProgramId, SettingType.AuthorityUserLicenseTotalCount));
            else
            {
                //Setting will be at the Authority of this Industry
                var thisIndustry = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId);
                var authority = _dbContext.OrganizationRegulatoryPrograms
                    .Single(o => o.OrganizationId == thisIndustry.RegulatorOrganizationId &&
                    o.RegulatoryProgramId == thisIndustry.RegulatoryProgramId);

                maxCount = Convert.ToInt32(_settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.UserPerIndustryMaxCount));
            }
            var currentProgramUserCount = _dbContext.OrganizationRegulatoryProgramUsers
                                                    .Count(u => u.OrganizationRegulatoryProgramId == orgRegProgramId
                                                    && u.IsRemoved != true && u.IsRegistrationApproved == true);
            var remaining = maxCount - currentProgramUserCount;

            //Handle at caller
            //if (remaining < 0)
            //    throw new Exception(string.Format("ERROR: Remaining user license count is a negative number (={0}) for Org Reg Program={1}, IsForAuthority={2}", remaining, orgRegProgramId, isForAuthority));

            return remaining;

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
                                                    && u.IsRemoved != true && u.IsRegistrationApproved == true);
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
            OrganizationRegulatoryProgramDto authorityDto;
            var org = _dbContext.OrganizationRegulatoryPrograms.Include("Organization")
                .Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId);
            OrganizationRegulatoryProgram authority;
            if (org.RegulatorOrganization != null)
            {
                authority = _dbContext.OrganizationRegulatoryPrograms.Include("Organization")
                    .Single(o => o.OrganizationId == org.RegulatorOrganization.OrganizationId
                    && o.RegulatoryProgramId == org.RegulatoryProgramId);

            }
            else
                authority = org;

            authorityDto = _mapper.Map<OrganizationRegulatoryProgram, OrganizationRegulatoryProgramDto>(authority);
            return authorityDto;
        }

    }
}
 