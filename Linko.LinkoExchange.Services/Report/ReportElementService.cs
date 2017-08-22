using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.TimeZone;
using NLog;

namespace Linko.LinkoExchange.Services.Report
{
    public class ReportElementService : IReportElementService
    {
        #region fields

        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContext;
        private readonly ILogger _logger;
        private readonly IMapHelper _mapHelper;
        private readonly IOrganizationService _orgService;
        private readonly ITimeZoneService _timeZoneService;

        #endregion

        #region constructors and destructor

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

        #endregion

        #region interface implementations

        /// <summary>
        ///     Gets a collection of Report Element Types associated with a category.
        /// </summary>
        /// <param name="categoryName"> The name of the report element category </param>
        /// <returns> Collection of dto's that map to the Report Element Type objects associated with the passed in category </returns>
        public IEnumerable<ReportElementTypeDto> GetReportElementTypes(ReportElementCategoryName categoryName)
        {
            _logger.Info(message:$"Enter ReportElementService.GetReportElementTypes. categoryName={categoryName}");

            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var authOrgRegProgramId = _orgService.GetAuthority(orgRegProgramId:currentOrgRegProgramId).OrganizationRegulatoryProgramId;
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
                var dto = _mapHelper.GetReportElementTypeDtoFromReportElementType(reportElementType:reportElementType);
                dto.LastModificationDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:reportElementType.LastModificationDateTimeUtc.HasValue
                                                                                ? reportElementType.LastModificationDateTimeUtc.Value.UtcDateTime
                                                                                : reportElementType.CreationDateTimeUtc.UtcDateTime, orgRegProgramId:currentOrgRegProgramId);

                if (reportElementType.LastModifierUserId.HasValue)
                {
                    var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == reportElementType.LastModifierUserId.Value);
                    dto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
                }
                else
                {
                    dto.LastModifierFullName = "N/A";
                }

                reportElementTypes.Add(item:dto);
            }

            _logger.Info(message:$"Leaving ReportElementService.GetReportElementTypes. reportElementTypes.Count={reportElementTypes.Count()}");

            return reportElementTypes;
        }

        /// <summary>
        ///     Gets the details of a Report Element Type from the database.
        /// </summary>
        /// <param name="reportElementTypeId"> ReportElementTypeId in the tReportElementType table </param>
        /// <returns> Dto that maps to the Report Element Type object associated with the passed in Id </returns>
        public ReportElementTypeDto GetReportElementType(int reportElementTypeId)
        {
            _logger.Info(message:$"Enter ReportElementService.GetReportElementType. reportElementTypeId={reportElementTypeId}");

            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var foundREType = _dbContext.ReportElementTypes
                                        .Single(re => re.ReportElementTypeId == reportElementTypeId);

            if (foundREType.OrganizationRegulatoryProgramId != currentOrgRegProgramId)
            {
                throw new UnauthorizedAccessException();
            }

            var dto = _mapHelper.GetReportElementTypeDtoFromReportElementType(reportElementType:foundREType);

            //Set LastModificationDateTimeLocal
            dto.LastModificationDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:foundREType.LastModificationDateTimeUtc.HasValue
                                                                            ? foundREType.LastModificationDateTimeUtc.Value.UtcDateTime
                                                                            : foundREType.CreationDateTimeUtc.UtcDateTime, orgRegProgramId:currentOrgRegProgramId);

            if (foundREType.LastModifierUserId.HasValue)
            {
                var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == foundREType.LastModifierUserId.Value);
                dto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
            }
            else
            {
                dto.LastModifierFullName = "N/A";
            }

            _logger.Info(message:$"Leaving ReportElementService.GetReportElementType. reportElementTypeId={reportElementTypeId}");

            return dto;
        }

        /// <summary>
        ///     Creates a new Report Element Type in the database or updates an existing one (if an Id is provided)
        /// </summary>
        /// <param name="reportElementType">
        ///     The Dto that gets mapped to a Report Element Type and saved to the DB.
        ///     If and Id is not provided, it is assumed a new object gets created in the database.
        /// </param>
        /// <returns> Existing Id or newly created Id </returns>
        public int SaveReportElementType(ReportElementTypeDto reportElementType)
        {
            var reportElementTypeIdString = string.Empty;
            if (reportElementType.ReportElementTypeId.HasValue)
            {
                reportElementTypeIdString = reportElementType.ReportElementTypeId.Value.ToString();
            }
            else
            {
                reportElementTypeIdString = "null";
            }

            _logger.Info(message:$"Enter ReportElementService.SaveReportElementType. reportElementType.ReportElementTypeId.Value={reportElementTypeIdString}");

            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var authOrgRegProgramId = _orgService.GetAuthority(orgRegProgramId:currentOrgRegProgramId).OrganizationRegulatoryProgramId;
            var currentUserId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.UserProfileId));
            var reportElementTypeIdToReturn = -1;
            var validationIssues = new List<RuleViolation>();

            //Check required fields (Name and Certification Text as per UC-53.3 4.b.)
            if (string.IsNullOrWhiteSpace(value:reportElementType.Name))
            {
                var message = "Name is required.";
                validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
            }

            if (reportElementType.ReportElementCategory == ReportElementCategoryName.Certifications
                && string.IsNullOrWhiteSpace(value:reportElementType.Content))
            {
                var message = "Certification Text is required.";
                validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
            }

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    //Find existing element types with same Name (UC-53-3 4.c.)
                    var proposedElementTypeName = reportElementType.Name.Trim().ToLower();
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
                                var message = "A Report Element Type with that name already exists. Please select another name.";
                                validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                                throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                            }
                        }

                        //Update existing
                        reportElementTypeToPersist = _dbContext.ReportElementTypes.Single(c => c.ReportElementTypeId == reportElementType.ReportElementTypeId);
                        reportElementTypeToPersist =
                            _mapHelper.GetReportElementTypeFromReportElementTypeDto(reportElementTypeDto:reportElementType, existingReportElementType:reportElementTypeToPersist);
                        reportElementTypeToPersist.OrganizationRegulatoryProgramId = currentOrgRegProgramId;
                        reportElementTypeToPersist.ReportElementCategoryId = _dbContext.ReportElementCategories
                                                                                       .Single(cat => cat.Name == reportElementType.ReportElementCategory.ToString())
                                                                                       .ReportElementCategoryId;
                        reportElementTypeToPersist.LastModificationDateTimeUtc = DateTimeOffset.Now;
                        reportElementTypeToPersist.LastModifierUserId = currentUserId;
                    }
                    else
                    {
                        //Ensure there are no other element types with same name
                        if (elementTypesWithMatchingName.Count() > 0)
                        {
                            var message = "A Report Element Type with that name already exists. Please select another name.";
                            validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                            throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                        }

                        //Get new
                        reportElementTypeToPersist = _mapHelper.GetReportElementTypeFromReportElementTypeDto(reportElementTypeDto:reportElementType);
                        reportElementTypeToPersist.OrganizationRegulatoryProgramId = currentOrgRegProgramId;
                        reportElementTypeToPersist.ReportElementCategoryId = _dbContext.ReportElementCategories
                                                                                       .Single(cat => cat.Name == reportElementType.ReportElementCategory.ToString())
                                                                                       .ReportElementCategoryId;
                        reportElementTypeToPersist.CreationDateTimeUtc = DateTimeOffset.Now;
                        reportElementTypeToPersist.LastModificationDateTimeUtc = DateTimeOffset.Now;
                        reportElementTypeToPersist.LastModifierUserId = currentUserId;
                        _dbContext.ReportElementTypes.Add(entity:reportElementTypeToPersist);
                    }

                    _dbContext.SaveChanges();

                    reportElementTypeIdToReturn = reportElementTypeToPersist.ReportElementTypeId;

                    transaction.Commit();
                }
                catch (RuleViolationException)
                {
                    transaction.Rollback();
                    throw;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            _logger.Info(message:$"Leaving ReportElementService.SaveReportElementType. reportElementTypeIdToReturn={reportElementTypeIdToReturn}");

            return reportElementTypeIdToReturn;
        }

        /// <summary>
        ///     Removes a Report Element Type from the database
        /// </summary>
        /// <param name="reportElementTypeId"> Id of the Report Element Type to remove from the database </param>
        public ReportElementTypeDto DeleteReportElementType(int reportElementTypeId)
        {
            _logger.Info(message:$"Enter ReportElementService.DeleteReportElementType. reportElementTypeId={reportElementTypeId}");

            var authOrgRegProgramId = _orgService.GetAuthority(orgRegProgramId:int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId)))
                                                 .OrganizationRegulatoryProgramId;
            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    //Find all Report Package Templates using this Report Element Type
                    var rpTemplatesUsingThis = _dbContext.ReportPackageTemplateElementTypes
                                                         .Include(r => r.ReportPackageTemplateElementCategory)
                                                         .Where(r => r.ReportElementTypeId == reportElementTypeId)
                                                         .Select(r => r.ReportPackageTemplateElementCategory.ReportPackageTemplate)
                                                         .Where(r => r.OrganizationRegulatoryProgramId == authOrgRegProgramId);

                    if (rpTemplatesUsingThis.Count() > 0)
                    {
                        var warningMessage = "This Report Package Element is in use in the following Report Package Templates and cannot be deleted:";
                        var validationIssues = new List<RuleViolation>();
                        validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:warningMessage));

                        foreach (var rpTemplate in rpTemplatesUsingThis)
                        {
                            warningMessage = $" - \"{rpTemplate.Name}\"";
                            validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:warningMessage));
                        }

                        throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                    }

                    var foundReportElementType = _dbContext.ReportElementTypes
                                                           .Single(r => r.ReportElementTypeId == reportElementTypeId);

                    var elementTypeDto = _mapHelper.GetReportElementTypeDtoFromReportElementType(reportElementType:foundReportElementType);
                    _dbContext.ReportElementTypes.Remove(entity:foundReportElementType);

                    _dbContext.SaveChanges();
                    transaction.Commit();

                    _logger.Info(message:$"Leaving ReportElementService.DeleteReportElementType. reportElementTypeId={reportElementTypeId}");

                    return elementTypeDto;
                }
                catch (RuleViolationException)
                {
                    transaction.Rollback();
                    throw;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        ///     Checks to see if a Report Element Type is used in any Report Package Template
        /// </summary>
        /// <param name="reportElementTypeId"> The Id of the Report Element Type to check. </param>
        /// <returns> True = Report Element Type is included in at least 1 Report Package Template, False otherwise. </returns>
        public bool IsReportElementTypeInUse(int reportElementTypeId)
        {
            bool isInUse;
            _logger.Info(message:$"Enter ReportElementService.IsReportElementTypeInUse. reportElementTypeId={reportElementTypeId}");

            var authOrgRegProgramId = _orgService.GetAuthority(orgRegProgramId:int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId)))
                                                 .OrganizationRegulatoryProgramId;

            //Find all Report Package Templates using this Report Element Type
            var rpTemplatesUsingThis = _dbContext.ReportPackageTemplateElementTypes
                                                 .Include(r => r.ReportPackageTemplateElementCategory)
                                                 .Where(r => r.ReportElementTypeId == reportElementTypeId)
                                                 .Select(r => r.ReportPackageTemplateElementCategory.ReportPackageTemplate)
                                                 .Where(r => r.OrganizationRegulatoryProgramId == authOrgRegProgramId);

            isInUse = rpTemplatesUsingThis.Any();
            _logger.Info(message:$"Leaving ReportElementService.IsReportElementTypeInUse. reportElementTypeId={reportElementTypeId}, isInUse={isInUse}");

            return isInUse;
        }

        #endregion
    }
}