using AutoMapper;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Settings;

namespace Linko.LinkoExchange.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly LinkoExchangeContext _dbContext;
        //private readonly IAuditLogEntry _logger;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;
        private readonly ISettingService _settingService;

        public OrganizationService(LinkoExchangeContext dbContext, IMapper mapper,
            ICurrentUser currentUser, ISettingService settingService)//, IAuditLogEntry logger)
        {
            _dbContext = dbContext;
            //_logger = logger;
            _mapper = mapper;
            _currentUser = currentUser;
            _settingService = settingService;
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
            int userProfileId = Convert.ToInt32(_currentUser.UserProfileId());
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
                    AuthorityDto authority = _mapper.Map<Core.Domain.Organization, AuthorityDto>((Core.Domain.Organization)orpUser.OrganizationRegulatoryProgram.Organization);
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
        public void UpdateEnableDisableFlag(int orgRegProgId, bool isEnabled)
        {
            try
            {
                var orgRegProg = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgId);
                orgRegProg.IsEnabled = isEnabled;
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

        public OrganizationRegulatoryProgramDto GetOrganizationRegulatoryProgram(int orgRegProgId)
        {
            var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgId);
            OrganizationRegulatoryProgramDto dto = _mapper.Map<OrganizationRegulatoryProgram, OrganizationRegulatoryProgramDto>(orgRegProgram);
            dto.HasSignatory = _dbContext.OrganizationRegulatoryProgramUsers
                .Count(o => o.OrganizationRegulatoryProgramId == orgRegProgram.OrganizationRegulatoryProgramId
                && o.IsSignatory == true) > 0;

            return dto;
        }

        public List<OrganizationRegulatoryProgramDto> GetChildOrganizationRegulatoryPrograms(int orgRegProgId)
        {
            try
            {
                var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgId);
                var childOrgRegProgs = _dbContext.OrganizationRegulatoryPrograms.Where(o => o.RegulatorOrganizationId == orgRegProgram.OrganizationId
                    && o.RegulatoryProgramId == orgRegProgram.RegulatoryProgramId);
                if (childOrgRegProgs != null)
                {
                    var dtoList = new List<OrganizationRegulatoryProgramDto>();
                    foreach (var orgRegProg in childOrgRegProgs.ToList())
                    {
                        OrganizationRegulatoryProgramDto dto = _mapper.Map<OrganizationRegulatoryProgram, OrganizationRegulatoryProgramDto>(orgRegProg);
                        dto.HasSignatory = _dbContext.OrganizationRegulatoryProgramUsers
                            .Count(o => o.OrganizationRegulatoryProgramId == orgRegProg.OrganizationRegulatoryProgramId
                            && o.IsSignatory == true) > 0;
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

    }
}
 