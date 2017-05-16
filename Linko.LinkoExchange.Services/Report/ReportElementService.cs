using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Mapping;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Linko.LinkoExchange.Services.Dto;
using System.Data.Entity;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.TimeZone;

namespace Linko.LinkoExchange.Services.Report
{
    public class ReportElementService : IReportElementService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContext;
        private readonly IOrganizationService _orgService;
        private readonly IMapHelper _mapHelper;
        private readonly ILogger _logger;
        private readonly ITimeZoneService _timeZoneService;

        public ReportElementService(LinkoExchangeContext dbContext,
            IHttpContextService httpContext,
            IOrganizationService orgService,
            IMapHelper mapHelper,
            ILogger logger,
            ITimeZoneService timeZoneService)
        {
            _dbContext = dbContext;
            _httpContext = httpContext;
            _orgService = orgService;
            _mapHelper = mapHelper;
            _logger = logger;
            _timeZoneService = timeZoneService;
        }

        /// <summary>
        /// Gets a collection of Report Element Types associated with a category.
        /// </summary>
        /// <param name="categoryName">The name of the report element category</param>
        /// <returns>Collection of dto's that map to the Report Element Type objects associated with the passed in category </returns>
        public IEnumerable<ReportElementTypeDto> GetReportElementTypes(ReportElementCategoryName categoryName)
        {
            _logger.Info($"Enter ReportElementService.GetReportElementTypes. categoryName={categoryName}");

            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var authOrgRegProgramId = _orgService.GetAuthority(currentOrgRegProgramId).OrganizationRegulatoryProgramId;
            var reportElementTypes = new List<ReportElementTypeDto>();
            var reportElementCategoryId = _dbContext.ReportElementCategories
                .Single(r => r.Name == categoryName.ToString()).ReportElementCategoryId;

            var foundReportElementTypes = _dbContext.ReportElementTypes
                .Include(c => c.CtsEventType)
                .Where(c => c.OrganizationRegulatoryProgramId == authOrgRegProgramId
                    && c.ReportElementCategoryId == reportElementCategoryId)
                .ToList();
            foreach (var reportElementType in foundReportElementTypes)
            {
                var dto = _mapHelper.GetReportElementTypeDtoFromReportElementType(reportElementType);
                dto.LastModificationDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingSettingForThisOrg((reportElementType.LastModificationDateTimeUtc.HasValue ? reportElementType.LastModificationDateTimeUtc.Value.UtcDateTime 
                                                                    : reportElementType.CreationDateTimeUtc.UtcDateTime), currentOrgRegProgramId);

                if (reportElementType.LastModifierUserId.HasValue)
                {
                    var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == reportElementType.LastModifierUserId.Value);
                    dto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
                }
                else
                {
                    dto.LastModifierFullName = "N/A";
                }

                reportElementTypes.Add(dto);
            }

            _logger.Info($"Leaving ReportElementService.GetReportElementTypes. reportElementTypes.Count={reportElementTypes.Count()}");

            return reportElementTypes;
        }

        /// <summary>
        /// Gets the details of a Report Element Type from the database.
        /// </summary>
        /// <param name="reportElementTypeId">ReportElementTypeId in the tReportElementType table</param>
        /// <returns>Dto that maps to the Report Element Type object associated with the passed in Id</returns>
        public ReportElementTypeDto GetReportElementType(int reportElementTypeId)
        {
            _logger.Info($"Enter ReportElementService.GetReportElementType. reportElementTypeId={reportElementTypeId}");

            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var foundREType = _dbContext.ReportElementTypes
                .Single(re => re.ReportElementTypeId == reportElementTypeId);

            if (foundREType.OrganizationRegulatoryProgramId != currentOrgRegProgramId)
            {
                throw new UnauthorizedAccessException();
            }

            var dto = _mapHelper.GetReportElementTypeDtoFromReportElementType(foundREType);

            //Set LastModificationDateTimeLocal
            dto.LastModificationDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingSettingForThisOrg((foundREType.LastModificationDateTimeUtc.HasValue ? foundREType.LastModificationDateTimeUtc.Value.UtcDateTime
                        : foundREType.CreationDateTimeUtc.UtcDateTime), currentOrgRegProgramId);

            if (foundREType.LastModifierUserId.HasValue)
            {
                var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == foundREType.LastModifierUserId.Value);
                dto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
            }
            else
            {
                dto.LastModifierFullName = "N/A";
            }

            _logger.Info($"Leaving ReportElementService.GetReportElementType. reportElementTypeId={reportElementTypeId}");

            return dto;
        }

        /// <summary>
        /// Creates a new Report Element Type in the database or updates an existing one (if an Id is provided)
        /// </summary>
        /// <param name="reportElementType">The Dto that gets mapped to a Report Element Type and saved to the DB.
        /// If and Id is not provided, it is assumed a new object gets created in the database.</param>
        /// <returns>Existing Id or newly created Id</returns>
        public int SaveReportElementType(ReportElementTypeDto reportElementType)
        {
            string reportElementTypeIdString = string.Empty;
            if (reportElementType.ReportElementTypeId.HasValue)
            {
                reportElementTypeIdString = reportElementType.ReportElementTypeId.Value.ToString();
            }
            else
            {
                reportElementTypeIdString = "null";
            }

            _logger.Info($"Enter ReportElementService.SaveReportElementType. reportElementType.ReportElementTypeId.Value={reportElementTypeIdString}");

            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var authOrgRegProgramId = _orgService.GetAuthority(currentOrgRegProgramId).OrganizationRegulatoryProgramId;
            var currentUserId = int.Parse(_httpContext.GetClaimValue(CacheKey.UserProfileId));
            var reportElementTypeIdToReturn = -1;
            List<RuleViolation> validationIssues = new List<RuleViolation>();

            //Check required fields (Name and Certification Text as per UC-53.3 4.b.)
            if (string.IsNullOrWhiteSpace(reportElementType.Name))
            {
                string message = "Name is required.";
                validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
            }
            
            if (reportElementType.ReportElementCategory == ReportElementCategoryName.Certifications
                && string.IsNullOrWhiteSpace(reportElementType.Content))
            {
                string message = "Certification Text is required.";
                validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
            }

            using (var transaction = _dbContext.BeginTransaction())
            {
                try {
                    //Find existing element types with same Name (UC-53-3 4.c.)
                    string proposedElementTypeName = reportElementType.Name.Trim().ToLower();
                    var elementTypesWithMatchingName = _dbContext.ReportElementTypes
                        .Where(ret => ret.Name.Trim().ToLower() == proposedElementTypeName
                                && ret.OrganizationRegulatoryProgramId == authOrgRegProgramId);

                    ReportElementType reportElementTypeToPersist = null;

                    if (reportElementType.ReportElementTypeId.HasValue && reportElementType.ReportElementTypeId.Value > 0)
                    {
                        //Ensure there are no other element types with same name
                        foreach (var elementTypeWithMatchingName in elementTypesWithMatchingName)
                        {
                            if (elementTypeWithMatchingName.ReportElementTypeId != reportElementType.ReportElementTypeId.Value)
                            {
                                string message = "A Report Element Type with that name already exists. Please select another name.";
                                validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                                throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                            }
                        }

                        //Update existing
                        reportElementTypeToPersist = _dbContext.ReportElementTypes.Single(c => c.ReportElementTypeId == reportElementType.ReportElementTypeId);
                        reportElementTypeToPersist = _mapHelper.GetReportElementTypeFromReportElementTypeDto(reportElementType, reportElementTypeToPersist);
                        reportElementTypeToPersist.OrganizationRegulatoryProgramId = currentOrgRegProgramId;
                        reportElementTypeToPersist.ReportElementCategoryId = _dbContext.ReportElementCategories
                                                .Single(cat => cat.Name == reportElementType.ReportElementCategory.ToString()).ReportElementCategoryId;
                        reportElementTypeToPersist.LastModificationDateTimeUtc = DateTimeOffset.Now;
                        reportElementTypeToPersist.LastModifierUserId = currentUserId;

                    }
                    else
                    {
                        //Ensure there are no other element types with same name
                        if (elementTypesWithMatchingName.Count() > 0)
                        {
                            string message = "A Report Element Type with that name already exists. Please select another name.";
                            validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                            throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                        }

                        //Get new
                        reportElementTypeToPersist = _mapHelper.GetReportElementTypeFromReportElementTypeDto(reportElementType);
                        reportElementTypeToPersist.OrganizationRegulatoryProgramId = currentOrgRegProgramId;
                        reportElementTypeToPersist.ReportElementCategoryId = _dbContext.ReportElementCategories
                                                .Single(cat => cat.Name == reportElementType.ReportElementCategory.ToString()).ReportElementCategoryId;
                        reportElementTypeToPersist.CreationDateTimeUtc = DateTimeOffset.Now;
                        reportElementTypeToPersist.LastModificationDateTimeUtc = DateTimeOffset.Now;
                        reportElementTypeToPersist.LastModifierUserId = currentUserId;
                        _dbContext.ReportElementTypes.Add(reportElementTypeToPersist);
                    }
                    _dbContext.SaveChanges();

                    reportElementTypeIdToReturn = reportElementTypeToPersist.ReportElementTypeId;

                    transaction.Commit();
                }
                catch (RuleViolationException ex)
                {
                    transaction.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    var errors = new List<string>() { ex.Message };

                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        errors.Add(ex.Message);
                    }

                    _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));

                    throw;
                }

            }

            _logger.Info($"Leaving ReportElementService.SaveReportElementType. reportElementTypeIdToReturn={reportElementTypeIdToReturn}");

            return reportElementTypeIdToReturn;
        }

        /// <summary>
        /// Removes a Report Element Type from the database
        /// </summary>
        /// <param name="reportElementTypeId">Id of the Report Element Type to remove from the database</param>
        public void DeleteReportElementType(int reportElementTypeId)
        {
            _logger.Info($"Enter ReportElementService.DeleteReportElementType. reportElementTypeId={reportElementTypeId}");

            var authOrgRegProgramId = _orgService.GetAuthority(int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId))).OrganizationRegulatoryProgramId;
            using (var transaction = _dbContext.BeginTransaction())
            {
                try {

                    //Find all Report Package Templates using this Report Element Type
                    var rpTemplatesUsingThis = _dbContext.ReportPackageTemplateElementTypes
                        .Include(r => r.ReportPackageTemplateElementCategory)
                        .Where(r => r.ReportElementTypeId == reportElementTypeId)
                            .Select(r => r.ReportPackageTemplateElementCategory.ReportPackageTemplate)
                            .Where(r => r.OrganizationRegulatoryProgramId == authOrgRegProgramId);

                    if (rpTemplatesUsingThis.Count() > 0)
                    {
                        string warningMessage = "This Report Package Element is in use in the following Report Package Templates and cannot be deleted:";
                        List<RuleViolation> validationIssues = new List<RuleViolation>();
                        validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: warningMessage));

                        foreach (var rpTemplate in rpTemplatesUsingThis)
                        {
                            warningMessage = $" - \"{rpTemplate.Name}\"";
                            validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage:  warningMessage));
                        }

                        throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                    }

                    var foundReportElementType = _dbContext.ReportElementTypes
                        .Single(r => r.ReportElementTypeId == reportElementTypeId);
                    _dbContext.ReportElementTypes.Remove(foundReportElementType);

                    _dbContext.SaveChanges();

                    transaction.Commit();
                }
                catch (RuleViolationException ex)
                {
                    transaction.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    var errors = new List<string>() { ex.Message };

                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        errors.Add(ex.Message);
                    }

                    _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));

                    throw;
                }

            }

            _logger.Info($"Leaving ReportElementService.DeleteReportElementType. reportElementTypeId={reportElementTypeId}");

        }

        /// <summary>
        /// Checks to see if a Report Element Type is used in any Report Package Template
        /// </summary>
        /// <param name="reportElementTypeId">The Id of the Report Element Type to check.</param>
        /// <returns>True = Report Element Type is included in at least 1 Report Package Template, False otherwise.</returns>
        public bool IsReportElementTypeInUse(int reportElementTypeId)
        {
            bool isInUse;
            _logger.Info($"Enter ReportElementService.IsReportElementTypeInUse. reportElementTypeId={reportElementTypeId}");

            var authOrgRegProgramId = _orgService.GetAuthority(int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId))).OrganizationRegulatoryProgramId;
            //Find all Report Package Templates using this Report Element Type
            var rpTemplatesUsingThis = _dbContext.ReportPackageTemplateElementTypes
                .Include(r => r.ReportPackageTemplateElementCategory)
                .Where(r => r.ReportElementTypeId == reportElementTypeId)
                    .Select(r => r.ReportPackageTemplateElementCategory.ReportPackageTemplate)
                    .Where(r => r.OrganizationRegulatoryProgramId == authOrgRegProgramId);


            if (rpTemplatesUsingThis.Count() > 0)
            {
                isInUse = true;
            }
            else
            {
                isInUse = false;
            }

            _logger.Info($"Leaving ReportElementService.IsReportElementTypeInUse. reportElementTypeId={reportElementTypeId}, isInUse={isInUse}");

            return isInUse;

        }
    }
}
