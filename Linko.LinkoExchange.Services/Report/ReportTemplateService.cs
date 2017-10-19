using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Base;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.User;
using NLog;

namespace Linko.LinkoExchange.Services.Report
{
    public class ReportTemplateService : BaseService, IReportTemplateService
    {
        #region fields

        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContextService;
        private readonly ILogger _logger;
        private readonly IMapHelper _mapHelper;
        private readonly IOrganizationService _orgService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IUserService _userService;
        private List<ReportPackageTemplateElementCategory> _reportPackageTemplateElementCategories;
        private List<ReportPackageTemplateElementType> _reportPackageTemplateElementTypes;

        #endregion

        #region constructors and destructor

        public ReportTemplateService(
            LinkoExchangeContext dbContext,
            IHttpContextService httpContextService,
            IUserService userService,
            IMapHelper mapHelper,
            ILogger logger,
            ITimeZoneService timeZoneService,
            IOrganizationService orgService)
        {
            _dbContext = dbContext;
            _httpContextService = httpContextService;
            _mapHelper = mapHelper;
            _logger = logger;
            _userService = userService;
            _timeZoneService = timeZoneService;
            _orgService = orgService;
        }

        #endregion

        #region interface implementations

        public override bool CanUserExecuteApi([CallerMemberName] string apiName = "", params int[] id)
        {
            var retVal = false;

            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var currentPortalName = _httpContextService.GetClaimValue(claimType:CacheKey.PortalName);
            currentPortalName = string.IsNullOrWhiteSpace(value:currentPortalName) ? "" : currentPortalName.Trim().ToLower();

            switch (apiName)
            {
                case "GetReportPackageTemplate":
                    var reportPackageTemplateId = id[0];
                    if (currentPortalName.Equals(value:OrganizationTypeName.Authority.ToString(), comparisonType: StringComparison.OrdinalIgnoreCase))
                    {
                        //this will also handle scenarios where ReportPackageTemplateId doesn't even exist (regardless of ownership)
                        var isTemplateForThisAuthorityExist = _dbContext.ReportPackageTempates
                                                                        .Any(rpt => rpt.ReportPackageTemplateId == reportPackageTemplateId
                                                                                    && rpt.OrganizationRegulatoryProgramId == currentOrgRegProgramId);

                        retVal = isTemplateForThisAuthorityExist;
                    }
                    else
                    {
                        var isTemplateAssigned = _dbContext.ReportPackageTemplateAssignments
                                                           .Any(rpta => rpta.OrganizationRegulatoryProgramId == currentOrgRegProgramId
                                                                        && rpta.ReportPackageTemplateId == reportPackageTemplateId);

                        retVal = isTemplateAssigned;
                    }

                    break;

                default: throw new Exception(message:$"ERROR: Unhandled API authorization attempt using name = '{apiName}'");
            }

            return retVal;
        }

        /// <summary>
        ///     Delete data from other tables before delete from  ReportPackageTempates
        ///     1. Delete from tReportPackageTemplateAssignment
        ///     2. Delete from tReportPackageTemplateElementCategory
        ///     2.1  Delete from  tReportPackageTemplateElementType table
        ///     2.2  Delete from  tReportPackageTemplateElementCategory table
        ///     3.  Delete from tReportPackageTempates
        /// </summary>
        /// <param name="reportPackageTemplateId"> </param>
        public void DeleteReportPackageTemplate(int reportPackageTemplateId)
        {
            _logger.Info(message:"Enter ReportTemplateService.DeleteReportPackageTemplate. reportPackageTemplateId={0}", argument:reportPackageTemplateId);
            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    var rpt =
                        _dbContext.ReportPackageTempates.FirstOrDefault(
                                                                        i => i.ReportPackageTemplateId == reportPackageTemplateId);
                    if (rpt == null)
                    {
                        return;
                    }

                    // Step 1, 2 
                    DeleteReportPackageChildrenObjects(rpt:rpt);

                    // Step 3
                    _dbContext.ReportPackageTempates.Remove(entity:rpt);
                    _dbContext.SaveChanges();
                    transaction.Commit();

                    _logger.Info(message:"Leave ReportTemplateService.DeleteReportPackageTemplate. reportPackageTemplateId={0}", argument:reportPackageTemplateId);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public ReportPackageTemplateDto GetReportPackageTemplate(int reportPackageTemplateId)
        {
            _logger.Info(message:"Enter ReportTemplateService.ReportPackageTemplateDto. reportPackageTemplateId={0}", argument:reportPackageTemplateId);

            if (!CanUserExecuteApi(id:reportPackageTemplateId))
            {
                throw new UnauthorizedAccessException();
            }

            var rpt =
                _dbContext.ReportPackageTempates.SingleOrDefault(
                                                                 i => i.ReportPackageTemplateId == reportPackageTemplateId);

            _reportPackageTemplateElementCategories = _dbContext.ReportPackageTemplateElementCategories
                                                                .Where(a => a.ReportPackageTemplateId == reportPackageTemplateId).ToList();

            _reportPackageTemplateElementTypes = _dbContext.ReportPackageTemplateElementTypes.Include(a => a.ReportPackageTemplateElementCategory)
                                                           .Where(b => b.ReportPackageTemplateElementCategory.ReportPackageTemplateId == reportPackageTemplateId).ToList();

            var rptDto = GetReportPackageTemplateInner(rpt:rpt);

            _logger.Info(message:"Enter ReportTemplateService.ReportPackageTemplateDto.");
            return rptDto;
        }

        public IEnumerable<ReportPackageTemplateDto> GetReportPackageTemplates(bool isForCreatingDraft = false, bool includeChildObjects = true)
        {
            _logger.Info(message:"Enter ReportTemplateService.GetReportPackageTemplates.");

            var currentRegulatoryProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

            var rptsQuery = _dbContext.ReportPackageTempates
                                      .Include(a => a.CtsEventType)
                                      .Include(a => a.OrganizationRegulatoryProgram)
                                      .Include(a => a.ReportPackageTemplateAssignments.Select(b => b.OrganizationRegulatoryProgram))
                                      .Include(a => a.ReportPackageTemplateElementCategories.Select(b => b.ReportElementCategory))
                                      .Include(a => a.ReportPackageTemplateElementCategories.Select(b => b.ReportPackageTemplateElementTypes.Select(d => d.ReportElementType)));

            if (isForCreatingDraft)
            {
                rptsQuery = rptsQuery.Where(i => i.IsActive
                                                 && i.EffectiveDateTimeUtc <= DateTimeOffset.UtcNow
                                                 && i.ReportPackageTemplateAssignments.Select(j => j.OrganizationRegulatoryProgram.OrganizationRegulatoryProgramId)

                                                     // ReSharper disable once ArgumentsStyleNamedExpression
                                                     .Contains(currentRegulatoryProgramId))
                                     .GroupBy(x => x.Name, (key, g) => g.OrderByDescending(e => e.EffectiveDateTimeUtc).FirstOrDefault());

                //the last line of query ensures we only get the newest template when there are multiple with the same name (UC-16.4 1.1.4.)
            }
            else
            {
                rptsQuery = rptsQuery
                    .Where(i => i.OrganizationRegulatoryProgramId == currentRegulatoryProgramId);
            }

            var rpts = rptsQuery.Distinct().ToArray();

            if (includeChildObjects)
            {
                _reportPackageTemplateElementCategories = _dbContext.ReportPackageTemplateElementCategories.Include(i => i.ReportPackageTemplate)
                                                                    .Where(a => a.ReportPackageTemplate.OrganizationRegulatoryProgramId == currentRegulatoryProgramId).ToList();

                _reportPackageTemplateElementTypes = _dbContext.ReportPackageTemplateElementTypes.Include(a => a.ReportPackageTemplateElementCategory.ReportPackageTemplate)
                                                               .Where(b => b.ReportPackageTemplateElementCategory.ReportPackageTemplate.OrganizationRegulatoryProgramId
                                                                           == currentRegulatoryProgramId)
                                                               .ToList();
            }
            var rptDtos = rpts.Select(rpt => GetReportPackageTemplateInner(rpt:rpt, includeChildObjects:includeChildObjects)).ToList();

            _logger.Info(message:"Enter ReportTemplateService.ReportPackageTemplateDto. Return count={0}", argument:rptDtos.Count);
            return rptDtos;
        }

        /// <summary>
        ///     This function provides update existing one, and create a new one functionalities
        ///     For update, after update tReportPackageTemplate table, do following steps
        ///     1 Delete from tReprotPackageTemplateAssignment table,
        ///     2 Delete from tReportPackageTemplateElementType table,
        ///     3 Delete from tReportPackageTemplateElementCategory table,
        ///     4 Create records in tReprotPackageTemplateAssignment table,
        ///     5 Create records in tReportPackageTemplateElementCategory table,
        ///     6 Create records in tReportPackageTemplateElementElementType table
        /// 
        ///     For new creation, do following steps
        ///     1. Create on record in tReportPackageTempate,
        ///     2. Create one records in  tReprotPackageTemplateAssignment,
        ///     3. For AttachmentType and CertificationTypes, do below:
        ///     2.1 Create one record in tReportPackageTemplateElementCategory
        ///     2.2 Create records in table tReportPackageTemplateElementType
        /// </summary>
        /// <param name="rpt"> The ReportPackageTemplateDto Object </param>
        public int SaveReportPackageTemplate(ReportPackageTemplateDto rpt)
        {
            _logger.Info(message:"Enter ReportTemplateService.SaveReportPackageTemplate.");

            var rptId = rpt.ReportPackageTemplateId ?? -1;

            using (var transaction = _dbContext.BeginTransaction())
            {
                var validationIssues = new List<RuleViolation>();

                try
                {
                    var currentUserId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.UserProfileId));
                    var currentRegulatoryProgramId =
                        int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

                    //// Check report template name and effective date  
                    if (string.IsNullOrWhiteSpace(value:rpt.Name))
                    {
                        var message = "Name is required.";
                        validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                        throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                    }

                    rpt.Name = rpt.Name.Trim();

                    if (rpt.EffectiveDateTimeLocal == DateTimeOffset.MinValue)
                    {
                        var message = "EffectiveDateTimeUtc is required.";
                        validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                        throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                    }

                    var effectiveDateTimeUtc =
                        _timeZoneService.GetDateTimeOffsetFromLocalUsingThisOrg(localDateTime:rpt.EffectiveDateTimeLocal,
                                                                                orgRegProgramId:currentRegulatoryProgramId);

                    if (rpt.RetirementDateTimeLocal.HasValue)
                    {
                        rpt.RetirementDateTimeUtc =
                            _timeZoneService.GetDateTimeOffsetFromLocalUsingThisOrg(localDateTime:rpt.RetirementDateTimeLocal.Value, orgRegProgramId:currentRegulatoryProgramId);
                    }

                    //// Check if Name and EffectiveDateTimeUtc combination is unique or not 
                    var testRpt = _dbContext.ReportPackageTempates
                                            .FirstOrDefault(i => i.EffectiveDateTimeUtc == effectiveDateTimeUtc
                                                                 && i.Name == rpt.Name
                                                                 && i.OrganizationRegulatoryProgramId == currentRegulatoryProgramId);

                    if (testRpt != null)
                    {
                        //// Creating a new report package template.
                        //// Or Updating the existing one, but testRpt is another one.
                        if (rpt.ReportPackageTemplateId.HasValue == false || testRpt.ReportPackageTemplateId != rpt.ReportPackageTemplateId.Value)
                        {
                            var message =
                                "A Template with that Name already exists for this Effective Date. Select another name or effective date";
                            validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null,
                                                                        errorMessage:message));
                            throw new RuleViolationException(message:"Validation errors",
                                                             validationIssues:validationIssues);
                        }
                    }

                    var reportPackageTemplate = _mapHelper.GetReportPackageTemplateFromReportPackageTemplateDto(reportPackageTemplateDto:rpt);

                    if (rpt.ReportPackageTemplateId.HasValue)
                    {
                        var currentReportPackageTempalte =
                            _dbContext.ReportPackageTempates.Single(i => i.ReportPackageTemplateId == rpt.ReportPackageTemplateId.Value);

                        //// Update current reportPackageTampate 
                        currentReportPackageTempalte.Name = rpt.Name;
                        currentReportPackageTempalte.Description = rpt.Description;
                        currentReportPackageTempalte.EffectiveDateTimeUtc = effectiveDateTimeUtc;
                        currentReportPackageTempalte.RetirementDateTimeUtc = rpt.RetirementDateTimeUtc;
                        currentReportPackageTempalte.CtsEventTypeId = rpt.CtsEventTypeId;
                        currentReportPackageTempalte.OrganizationRegulatoryProgramId = currentRegulatoryProgramId;
                        currentReportPackageTempalte.IsActive = rpt.IsActive;
                        currentReportPackageTempalte.LastModificationDateTimeUtc = DateTimeOffset.Now;
                        currentReportPackageTempalte.LastModifierUserId = currentUserId;

                        //  1 Delete from tReprotPackageTemplateAssignment table,
                        //  2 Delete from tReportPackageTemplateElementType table,
                        //  3 Delete from tReportPackageTemplateElementCategory table 
                        DeleteReportPackageChildrenObjects(rpt:currentReportPackageTempalte);
                    }
                    else
                    {
                        // Create new in tReportPackageTempate table
                        reportPackageTemplate.OrganizationRegulatoryProgramId = currentRegulatoryProgramId;
                        reportPackageTemplate.IsActive = rpt.IsActive;
                        reportPackageTemplate.LastModificationDateTimeUtc = DateTimeOffset.Now;
                        reportPackageTemplate.EffectiveDateTimeUtc = effectiveDateTimeUtc;
                        reportPackageTemplate.LastModifierUserId = currentUserId;

                        // First time creation recorder DO NOT need to provide creation date time.
                        reportPackageTemplate = _dbContext.ReportPackageTempates.Add(entity:reportPackageTemplate);
                        _dbContext.SaveChanges();
                        rptId = reportPackageTemplate.ReportPackageTemplateId;
                    }

                    if (rpt.ReportPackageTemplateAssignments != null && rpt.ReportPackageTemplateAssignments.Count > 0)
                    {
                        foreach (var asDto in rpt.ReportPackageTemplateAssignments)
                        {
                            var assignment = new ReportPackageTemplateAssignment
                                             {
                                                 OrganizationRegulatoryProgramId = asDto.OrganizationRegulatoryProgramId,
                                                 ReportPackageTemplateId = rptId
                                             };

                            reportPackageTemplate.ReportPackageTemplateAssignments.Add(item:assignment);
                        }
                    }

                    // Create records in tReportTemplateAssignment table
                    foreach (var assignment in reportPackageTemplate.ReportPackageTemplateAssignments)
                    {
                        _dbContext.ReportPackageTemplateAssignments.Add(entity:assignment);
                    }

                    _dbContext.SaveChanges();

                    // Determine show sample result section or not
                    if (rpt.ShowSampleResults)
                    {
                        if (rpt.SamplesAndResultsTypes != null && rpt.SamplesAndResultsTypes.Count > 0)
                        {
                            var samplesAndResultsTypes = rpt.SamplesAndResultsTypes.ToArray();
                            CreateReportPackageElementCatergoryType(reportElementTypeDtos:samplesAndResultsTypes, reportPackageTemplate:reportPackageTemplate, setOrder:1);
                        }
                    }

                    // AttachmentType   
                    if (rpt.AttachmentTypes != null && rpt.AttachmentTypes.Count > 0)
                    {
                        var attachmentTypes = rpt.AttachmentTypes.ToArray();
                        CreateReportPackageElementCatergoryType(reportElementTypeDtos:attachmentTypes, reportPackageTemplate:reportPackageTemplate, setOrder:1);
                    }

                    // CertificationType  
                    if (rpt.CertificationTypes != null && rpt.CertificationTypes.Count > 0)
                    {
                        var certificationTypes = rpt.CertificationTypes.ToArray();
                        CreateReportPackageElementCatergoryType(reportElementTypeDtos:certificationTypes, reportPackageTemplate:reportPackageTemplate, setOrder:2);
                    }

                    _dbContext.SaveChanges();
                    transaction.Commit();

                    _logger.Info(message:"Leave ReportTemplateService.SaveReportPackageTemplate. rptId={0}", argument:rptId);

                    return rptId;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public IEnumerable<CtsEventTypeDto> GetCtsEventTypes(bool isForSample)
        {
            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var authOrgRegProgramId = _orgService.GetAuthority(orgRegProgramId:currentOrgRegProgramId).OrganizationRegulatoryProgramId;
            var ctsEventTypes =
                _dbContext.CtsEventTypes.Where(
                                               i =>
                                                   i.IsEnabled
                                                   && i.IsRemoved == false
                                                   && i.CtsEventCategoryName.ToLower() == "sample" == isForSample
                                                   && i.OrganizationRegulatoryProgramId == authOrgRegProgramId).ToList();

            return ctsEventTypes.Select(i => _mapHelper.GetCtsEventTypeDtoFromEventType(ctsEventType:i));
        }

        public IEnumerable<ReportElementTypeDto> GetCertificationTypes()
        {
            return ReportElementTypeDtos(categoryName:ReportElementCategoryName.Certifications.ToString());
        }

        public IEnumerable<ReportElementTypeDto> GetAttachmentTypes()
        {
            return ReportElementTypeDtos(categoryName:ReportElementCategoryName.Attachments.ToString());
        }

        public IEnumerable<ReportElementTypeDto> GetSampleAndResultTypes()
        {
            return ReportElementTypeDtos(categoryName:ReportElementCategoryName.SamplesAndResults.ToString());
        }

        public IEnumerable<ReportElementCategoryDto> GetReportElementCategories()
        {
            var recats = _dbContext.ReportElementCategories.ToList();
            return recats.Select(i => _mapHelper.GetReportElementCategoryDtoFromReportElementCategory(cat:i)).ToList();
        }

        public IEnumerable<ReportElementCategoryName> GetReportElementCategoryNames()
        {
            var recats = _dbContext.ReportElementCategories.ToList();
            return recats.Select(i => (ReportElementCategoryName) Enum.Parse(enumType:typeof(ReportElementCategoryName), value:i.Name)).ToList();
        }

        #endregion

        #region private section

        private IEnumerable<ReportElementTypeDto> ReportElementTypeDtos(string categoryName)
        {
            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var currentRegulatoryProgramId =
                int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

            var reportElementTypes = _dbContext.ReportElementTypes.Where(
                                                                         i => i.OrganizationRegulatoryProgramId == currentRegulatoryProgramId
                                                                              && i.ReportElementCategory.Name == categoryName
                                                                        )

                                               //.Select(_mapHelper.GetReportElementTypeDtoFromReportElementType)
                                               .ToList();

            var reportElementTypeDtos = new List<ReportElementTypeDto>();

            foreach (var rpt in reportElementTypes)
            {
                var reportElementTypeDto = _mapHelper.GetReportElementTypeDtoFromReportElementType(reportElementType:rpt);
                if (rpt.LastModificationDateTimeUtc.HasValue)
                {
                    reportElementTypeDto.LastModificationDateTimeLocal =
                        _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:rpt.LastModificationDateTimeUtc.Value.UtcDateTime,
                                                                                    orgRegProgramId:currentOrgRegProgramId);
                    var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == rpt.LastModifierUserId.Value);
                    reportElementTypeDto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
                }

                reportElementTypeDtos.Add(item:reportElementTypeDto);
            }

            return reportElementTypeDtos;
        }

        private void CreateReportPackageElementCatergoryType(ReportElementTypeDto[] reportElementTypeDtos,
                                                             ReportPackageTemplate reportPackageTemplate, int setOrder)
        {
            // Step 1, Save to tReportPackageTemplateElementCategory table
            var reportElementTypeId = reportElementTypeDtos[0].ReportElementTypeId;
            var reportElementCategoryId = _dbContext.ReportElementTypes.Single(cat => cat.ReportElementTypeId == reportElementTypeId).ReportElementCategoryId;
            var rptec = new ReportPackageTemplateElementCategory
                        {
                            ReportPackageTemplateId = reportPackageTemplate.ReportPackageTemplateId,
                            ReportElementCategoryId = reportElementCategoryId,
                            SortOrder = setOrder
                        };

            rptec = _dbContext.ReportPackageTemplateElementCategories.Add(entity:rptec);

            // Save changes to get Id; 
            _dbContext.SaveChanges();
            var rptecId = rptec.ReportPackageTemplateElementCategoryId;

            for (var index = 0; index < reportElementTypeDtos.Length; index++)
            {
                var elementTypeDto = reportElementTypeDtos[index];

                // Go Step 2,  Save to table tReportPackageTemplateElementType  
                if (elementTypeDto.ReportElementTypeId.HasValue == false)
                {
                    throw new Exception(message:"Invalid ReportElementType");
                }

                var rptet = new ReportPackageTemplateElementType
                            {
                                ReportPackageTemplateElementCategoryId = rptecId,
                                ReportElementTypeId = elementTypeDto.ReportElementTypeId.Value,
                                SortOrder = index
                            };

                _dbContext.ReportPackageTemplateElementTypes.Add(entity:rptet);
                _dbContext.SaveChanges();
            }
        }

        private ReportPackageTemplateDto GetReportPackageTemplateInner(ReportPackageTemplate rpt, bool includeChildObjects = true)
        {
            _logger.Info(message:"Enter ReportTemplateService.ReportPackageTemplateDto.");
            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var rptDto = _mapHelper.GetReportPackageTemplateDtoFromReportPackageTemplate(reportPackageTemplate:rpt);

            rptDto.LastModificationDateTimeLocal = _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(
                                                                                                               utcDateTime:rpt.LastModificationDateTimeUtc?.UtcDateTime
                                                                                                                           ?? rpt.CreationDateTimeUtc.UtcDateTime,
                                                                                                               orgRegProgramId:currentOrgRegProgramId);

            rptDto.EffectiveDateTimeLocal =
                _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:rpt.EffectiveDateTimeUtc.UtcDateTime, orgRegProgramId:currentOrgRegProgramId);
            if (rpt.RetirementDateTimeUtc.HasValue)
            {
                rptDto.RetirementDateTimeLocal =
                    _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:rpt.RetirementDateTimeUtc.Value.UtcDateTime, orgRegProgramId:currentOrgRegProgramId);
            }

            if (includeChildObjects)
            {
                // set SamplesAndResultsTypes, AttachmentTypes, and CertificationTypes
                rptDto.SamplesAndResultsTypes =
                    GetReportElementCategoryReportElementTypes(categoryName:ReportElementCategoryName.SamplesAndResults, rpt:rpt, currentOrgRegProgramId:currentOrgRegProgramId);
                rptDto.AttachmentTypes =
                    GetReportElementCategoryReportElementTypes(categoryName:ReportElementCategoryName.Attachments, rpt:rpt, currentOrgRegProgramId:currentOrgRegProgramId);
                rptDto.CertificationTypes = GetReportElementCategoryReportElementTypes(categoryName:ReportElementCategoryName.Certifications, rpt:rpt,
                                                                                       currentOrgRegProgramId:currentOrgRegProgramId);

                // set assingedIndustries  
                rptDto.ReportPackageTemplateAssignments = rptDto.ReportPackageTemplateAssignments;
            }
            else
            {
                rptDto.SamplesAndResultsTypes = new List<ReportElementTypeDto>();
                rptDto.AttachmentTypes = new List<ReportElementTypeDto>();
                rptDto.CertificationTypes = new List<ReportElementTypeDto>();
                rptDto.ReportPackageTemplateAssignments = new List<OrganizationRegulatoryProgramDto>();
            }

            if (rpt.LastModifierUserId.HasValue)
            {
                var lastModifierUser = _userService.GetUserProfileById(userProfileId:rpt.LastModifierUserId.Value);
                rptDto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
            }

            _logger.Info(message:"Leave ReportTemplateService.GetReportPackageTemplateInner.");
            return rptDto;
        }

        private List<ReportElementTypeDto> GetReportElementCategoryReportElementTypes(ReportElementCategoryName categoryName, ReportPackageTemplate rpt, int currentOrgRegProgramId)
        {
            var category = _reportPackageTemplateElementCategories.FirstOrDefault(i => i.ReportElementCategory.Name == categoryName.ToString()
                                                                                       && i.ReportPackageTemplateId == rpt.ReportPackageTemplateId);

            var reportElementTypeDtos = new List<ReportElementTypeDto>();

            if (category != null)
            {
                var reportElementTypes = GetReportElementCategoryReportElementTypesHelper(cat:category);
                foreach (var reportElementType in reportElementTypes)
                {
                    var retDto = _mapHelper.GetReportElementTypeDtoFromReportElementType(reportElementType:reportElementType);

                    //Set LastModificationDateTimeLocal
                    retDto.LastModificationDateTimeLocal = _timeZoneService
                        .GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:reportElementType.LastModificationDateTimeUtc?.UtcDateTime ?? reportElementType.CreationDateTimeUtc.UtcDateTime,
                                                                    orgRegProgramId:currentOrgRegProgramId);

                    if (reportElementType.LastModifierUserId.HasValue)
                    {
                        var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == reportElementType.LastModifierUserId.Value);
                        retDto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
                    }
                    else
                    {
                        retDto.LastModifierFullName = "N/A";
                    }

                    reportElementTypeDtos.Add(item:retDto);
                }
            }

            return reportElementTypeDtos;
        }

        private IEnumerable<ReportElementType> GetReportElementCategoryReportElementTypesHelper(ReportPackageTemplateElementCategory cat)
        {
            var reportPackageTemplateElementTypes = _reportPackageTemplateElementTypes.Where(
                                                                                             i =>
                                                                                                 i.ReportPackageTemplateElementCategoryId
                                                                                                 == cat.ReportPackageTemplateElementCategoryId)
                                                                                      .Distinct()
                                                                                      .OrderBy(i => i.SortOrder)
                                                                                      .ToList();

            var reportElementTypes = reportPackageTemplateElementTypes.Select(i => i.ReportElementType).Distinct().ToList();
            foreach (var ret in reportElementTypes)
            {
                ret.CtsEventType =
                    _dbContext.CtsEventTypes.FirstOrDefault(i => i.CtsEventTypeId == ret.CtsEventTypeId);
            }

            return reportElementTypes;
        }

        private void DeleteReportPackageTemplateAssignments(int reportTemplateId)
        {
            var assignments =
                _dbContext.ReportPackageTemplateAssignments.Where(i => i.ReportPackageTemplateId == reportTemplateId).ToList();

            foreach (var assignment in assignments)
            {
                _dbContext.ReportPackageTemplateAssignments.Remove(entity:assignment);
            }
        }

        private void DeleteReportPackageChildrenObjects(ReportPackageTemplate rpt)
        {
            DeleteReportPackageTemplateAssignments(reportTemplateId:rpt.ReportPackageTemplateId);

            var currentCategories =
                rpt.ReportPackageTemplateElementCategories.Where(
                                                                 i => i.ReportPackageTemplateId == rpt.ReportPackageTemplateId).ToList();
            foreach (var rptec in currentCategories)
            {
                var rptetId = rptec.ReportPackageTemplateElementCategoryId;
                var rptets =
                    _dbContext.ReportPackageTemplateElementTypes.Where(
                                                                       i => i.ReportPackageTemplateElementCategoryId == rptetId);

                foreach (var rptet in rptets)
                {
                    // Step 2.1
                    _dbContext.ReportPackageTemplateElementTypes.Remove(entity:rptet);
                }

                // Step 2.2
                var temp =
                    _dbContext.ReportPackageTemplateElementCategories.Single(
                                                                             i => i.ReportPackageTemplateElementCategoryId == rptec.ReportPackageTemplateElementCategoryId);

                _dbContext.ReportPackageTemplateElementCategories.Remove(entity:temp);
            }
        }

        public CtsEventTypeDto GetCtsEventType(int ctsEventTypeId)
        {
            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var ctsEventType = _dbContext.CtsEventTypes
                                         .Single(cts => cts.CtsEventTypeId == ctsEventTypeId);

            var dto = _mapHelper.GetCtsEventTypeDtoFromEventType(ctsEventType:ctsEventType);

            //Set LastModificationDateTimeLocal
            dto.LastModificationDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:ctsEventType.LastModificationDateTimeUtc.HasValue
                                                                            ? ctsEventType.LastModificationDateTimeUtc.Value.UtcDateTime
                                                                            : ctsEventType.CreationDateTimeUtc.UtcDateTime, orgRegProgramId:currentOrgRegProgramId);

            if (ctsEventType.LastModifierUserId.HasValue)
            {
                var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == ctsEventType.LastModifierUserId.Value);
                dto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
            }
            else
            {
                dto.LastModifierFullName = "N/A";
            }

            return dto;
        }

        #endregion
    }
}