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

namespace Linko.LinkoExchange.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly ApplicationDbContext _dbContext;
        //private readonly IAuditLogEntry _logger;
        private readonly IMapper _mapper;

        public OrganizationService(ApplicationDbContext dbContext, IMapper mapper)//, IAuditLogEntry logger)
        {
            _dbContext = dbContext;
            //_logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get organizations that a user can access to
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>Collection of organization</returns>
        public IEnumerable<OrganizationDto> GetUserOrganizations(int userId)
        {
            //TODO
            var list = new List<OrganizationDto>
            {
                new OrganizationDto
                {
                    OrganizationId = 1000,
                    OrganizationName = "Mock organization name"
                }
            };

            return list;
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
                returnDto = new Dto.OrganizationDto();
                returnDto.OrganizationName = foundOrg.Name;
                returnDto.AddressLine1 = foundOrg.AddressLine1;
                returnDto.AddressLine2 = foundOrg.AddressLine2;
                returnDto.City = foundOrg.City;
                returnDto.ZipCode = foundOrg.ZipCode;
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
                foundOrg = _mapper.Map<OrganizationDto, Organization>(organization);
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

        public void UpdateSettings(OrganizationSettingsDto settings)
        {
            //TODO
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

        public List<OrganizationDto> GetChildrenOrganizations(int regOrgId)
        {
            try
            {
                var orgRegProgs = _dbContext.OrganizationRegulatoryPrograms.Where(o => o.RegulatorOrganizationId == regOrgId);
                if (orgRegProgs != null)
                {
                    var dtoList = new List<OrganizationDto>();
                    foreach (var orgRegProg in orgRegProgs)
                    {
                        dtoList.Add(_mapper.Map<Organization, OrganizationDto>(orgRegProg.Organization));

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
                var newOrg = _mapper.Map<OrganizationDto, Organization>(childOrganization);

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