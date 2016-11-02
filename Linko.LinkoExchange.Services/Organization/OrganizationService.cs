﻿using AutoMapper;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Core.Extensions;
using Linko.LinkoExchange.Core.Common;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly LinkoExchangeContext _dbContext;
        //private readonly IAuditLogEntry _logger;
        private readonly IMapper _mapper;
        private readonly ISettingService _settingService;
        private readonly IHttpContextService _httpContext;

        public OrganizationService(LinkoExchangeContext dbContext, IMapper mapper,
            ISettingService settingService, IHttpContextService httpContext)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _settingService = settingService;
            _httpContext = httpContext;
        }

        public IEnumerable<OrganizationDto> GetUserOrganizationsByOrgRegProgUserId(int orgRegProgUserId)
        {
            var orgDtoList = new List<OrganizationDto>();
            var orgRegProgramId = _dbContext.OrganizationRegulatoryProgramUsers.Single(o => o.OrganizationRegulatoryProgramUserId == orgRegProgUserId).OrganizationRegulatoryProgramId;
            var regProgramId = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId).RegulatoryProgramId;
            var orgList = _dbContext.OrganizationRegulatoryPrograms.Include("Organization").Where(o => o.RegulatoryProgramId == regProgramId);
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
        /// Return all the programs' regulatory list for the user
        /// </summary>
        /// <param name="userId">The user Id</param>
        /// <returns>The organizationId </returns>
        public IEnumerable<AuthorityDto> GetUserRegulatories(int userId)
        {
            try
            {
                var orpUsers = _dbContext.OrganizationRegulatoryProgramUsers.ToList()
                    .FindAll(u => u.UserProfileId == userId &&
                                u.IsRemoved == false &&
                                u.IsEnabled == true &&
                                u.IsRegistrationApproved &&
                                u.OrganizationRegulatoryProgram.IsEnabled &&
                                u.OrganizationRegulatoryProgram.IsRemoved == false);

                var orgs = new List<AuthorityDto>();
                foreach (var orpUser in orpUsers)
                {
                    if (orpUser.OrganizationRegulatoryProgram?.Organization != null &&
                        orpUser.OrganizationRegulatoryProgram?.Organization.OrganizationType.Name == "Authority" &&
                        !orgs.Any(i => i.OrganizationId == orpUser.OrganizationRegulatoryProgram.RegulatorOrganizationId))
                    {
                        AuthorityDto authority = _mapper.Map<Core.Domain.Organization, AuthorityDto>(orpUser.OrganizationRegulatoryProgram.Organization);
                        authority.RegulatoryProgramId = orpUser.OrganizationRegulatoryProgram.RegulatoryProgramId;
                        authority.OrganizationRegulatoryProgramId = orpUser.OrganizationRegulatoryProgram.OrganizationRegulatoryProgramId;
                        authority.EmailContactInfoName = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoName);
                        authority.EmailContactInfoEmailAddress = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoEmailAddress);
                        authority.EmailContactInfoPhone = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoPhone);
                        orgs.Add(authority);
                    }

                }

                return orgs;
            }
            catch (DbEntityValidationException ex)
            {
                HandleEntityException(ex);
            }
            catch (Exception e)
            {
                var linkoException = new LinkoExchangeException();
                linkoException.ErrorType = LinkoExchangeError.DatabaseSetting;
                linkoException.Errors = new List<string> { e.Message };
                throw linkoException;
            }
            return null;
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
                var jurisdiction = GetJurisdictionById(foundOrg.JurisdictionId);
                returnDto.State = jurisdiction == null ? "" : jurisdiction.Name;
            }
            catch (DbEntityValidationException ex)
            {
                HandleEntityException(ex);
            }
            catch(Exception e)
            {
                var linkoException = new LinkoExchangeException();
                linkoException.ErrorType = LinkoExchangeError.OrganizationSetting;
                linkoException.Errors = new List<string> { e.Message };
                throw linkoException;
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
        public bool UpdateEnableDisableFlag(int orgRegProgId, bool isEnabled)
        {
            var orgRegProg = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgId);
            bool isAuthority = orgRegProg.RegulatorOrganizationId == null;
            if (isEnabled)
            {
                //check for number of licenses exceeds limit (different for Industry and Authority)
                var remainingLicenses = 0;
                if (isAuthority)
                    remainingLicenses = GetRemainingUserLicenseCount(orgRegProgId, true);
                else
                    remainingLicenses = GetRemainingIndustryLicenseCount(orgRegProgId);
                
                if (remainingLicenses < 1)
                    return false;

            }

            orgRegProg.IsEnabled = isEnabled;
            _dbContext.SaveChanges();

            return true;
        } 

        public OrganizationRegulatoryProgramDto GetOrganizationRegulatoryProgram(int orgRegProgId)
        {
            var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgId);
            OrganizationRegulatoryProgramDto dto = _mapper.Map<OrganizationRegulatoryProgram, OrganizationRegulatoryProgramDto>(orgRegProgram);
            dto.HasSignatory = _dbContext.OrganizationRegulatoryProgramUsers
                .Count(o => o.OrganizationRegulatoryProgramId == orgRegProgram.OrganizationRegulatoryProgramId
                && o.IsSignatory == true) > 0;

            dto.HasAdmin = _dbContext.OrganizationRegulatoryProgramUsers.Include("PermissionGroup")
                .Count(o => o.OrganizationRegulatoryProgramId == orgRegProgram.OrganizationRegulatoryProgramId
                && o.PermissionGroup.Name == "Administrator") > 0;
            dto.OrganizationDto.State = GetJurisdictionById(orgRegProgram.Organization.JurisdictionId).Name;

            return dto;
        }

        public List<OrganizationRegulatoryProgramDto> GetChildOrganizationRegulatoryPrograms(int orgRegProgId, string startsWith = null)
        {
            try
            {
                var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgId);
                var childOrgRegProgs = _dbContext.OrganizationRegulatoryPrograms.Where(o => o.RegulatorOrganizationId == orgRegProgram.OrganizationId
                    && o.RegulatoryProgramId == orgRegProgram.RegulatoryProgramId);

                if (childOrgRegProgs != null && !String.IsNullOrEmpty(startsWith))
                {
                    childOrgRegProgs = childOrgRegProgs.Where(x => x.Organization.Name.StartsWith(startsWith));
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
                        dto.OrganizationDto.State = GetJurisdictionById(orgRegProg.Organization.JurisdictionId).Name;
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

        public Jurisdiction GetJurisdictionById(int jurisdictionId)
        {
            return _dbContext.Jurisdictions.Single(j => j.JurisdictionId == jurisdictionId);
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


        /// <summary>
        /// Get remaining users for either program or total users across all programs for the entire organization
        /// </summary>
        /// <param name="isForProgramOnly"></param>
        /// <param name="id"></param>
        /// <returns></returns>
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
            var currentProgramUserCount = _dbContext.OrganizationRegulatoryProgramUsers.Count(u => u.OrganizationRegulatoryProgramId == orgRegProgramId);
            var remaining = maxCount - currentProgramUserCount;

            if (remaining < 0)
                throw new Exception(string.Format("ERROR: Remaining user license count is a negative number (={0}) for Org Reg Program={1}, IsForAuthority={2}", remaining, orgRegProgramId, isForAuthority));

            return remaining;

        }

        public int GetRemainingIndustryLicenseCount(int orgRegProgramId)
        {
            var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId);
            var currentChildIndustryCount = _dbContext.OrganizationRegulatoryPrograms
                .Count(u => u.RegulatorOrganizationId == orgRegProgram.OrganizationId
                && u.RegulatoryProgramId == orgRegProgram.RegulatoryProgramId);

            int maxCount = Convert.ToInt32(_settingService.GetOrgRegProgramSettingValue(orgRegProgramId, SettingType.IndustryUserLicenseTotalCount));
            var remaining = maxCount - currentChildIndustryCount;

            if (remaining < 0)
                throw new Exception(string.Format("ERROR: Remaining industry license count is a negative number (={0}) for Org Reg Program={1}", remaining, orgRegProgramId));

            return remaining;

        }

    }
}
 