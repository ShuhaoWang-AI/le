using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Dto;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;

namespace Linko.LinkoExchange.Services.Organization
{
    public class OrganizationService : IOrganizationService
    {
        private readonly ApplicationDbContext _dbContext;
        //private readonly IAuditLogEntry _logger;

        public OrganizationService(ApplicationDbContext dbContext)//, IAuditLogEntry logger)
        {
            _dbContext = dbContext;
            //_logger = logger;
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
            //TODO
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
            //TODO
        }

        public List<OrganizationDto> GetChildrenOrganizations(int regOrgId)
        {
            return new List<OrganizationDto>();
        }

        public void AddChildOrganization(int parentRegOrdId, OrganizationDto childOrganization)
        {
            //TODO
        }

    }
}