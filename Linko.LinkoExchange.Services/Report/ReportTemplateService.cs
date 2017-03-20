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
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.User;
using NLog;
using Linko.LinkoExchange.Services.TimeZone;

namespace Linko.LinkoExchange.Services.Report
{
    public class ReportTemplateService : IReportTemplateService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContextService;
        //private readonly IReportElementService _reportElementService;
        private readonly IMapHelper _mapHelper;
        private readonly ILogger _logger;
        private readonly UserService _userService;
        private readonly ITimeZoneService _timeZoneService;

        private int _currentOrgRegProgramId;

        public ReportTemplateService(
            LinkoExchangeContext dbContext,
            IHttpContextService httpContextService,
            UserService userService,
            IMapHelper mapHelper,
            ILogger logger,
            ITimeZoneService timeZoneService)
        {
            _dbContext = dbContext;
            _httpContextService = httpContextService;
            _mapHelper = mapHelper;
            _logger = logger;
            _userService = userService;
            _timeZoneService = timeZoneService;

            _currentOrgRegProgramId = int.Parse(httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
        }

        /// <summary>
        ///  Delete data from other tables before delete from  ReportPackageTempates 
        ///  1. Delete from tReportPackageTemplateAssignment
        ///  2. Delete from tReportPackageTemplateElementCategory 
        ///      2.1  Delete from  tReportPackageTemplateElementType table
        ///      2.2  Delete from  tReportPackageTemplateElementCategory table 
        ///  3.  Delete from tReportPackageTempates  
        /// </summary>
        /// <param name="reportPackageTemplateId"></param>

        public void DeleteReportPackageTemplate(int reportPackageTemplateId)
        {
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
                    DeleteReportPackageChildrenObjects(rpt);

                    // Step 3
                    _dbContext.ReportPackageTempates.Remove(rpt);

                    _dbContext.SaveChanges();
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

                    _logger.Error("Error happens {0} ", string.Join("," + Environment.NewLine, errors));
                    throw;
                }
            }
        }

        public ReportPackageTemplateDto GetReportPackageTemplate(int reportPackageTemplateId)
        {
            var rpt =
                _dbContext.ReportPackageTempates.SingleOrDefault(
                    i => i.ReportPackageTemplateId == reportPackageTemplateId);

            return GetReportOneReportPackageTemplate(rpt);
        }

        public IEnumerable<ReportPackageTemplateDto> GetReportPackageTemplates()
        {
            var currentRegulatoryProgramId =
                       int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

            var rpts =
                _dbContext.ReportPackageTempates.Where(i => i.OrganizationRegulatoryProgramId == currentRegulatoryProgramId)
                    .Include(i => i.ReportPackageTemplateElementCategories)
                    .Distinct()
                    .ToArray();

            return rpts.Select(GetReportOneReportPackageTemplate).ToList();
        }

        /// <summary>
        // This function provides update existing one, and create a new one functionalities 
        // 
        // For update, after update tReportPackageTemplate table, do following steps
        //  1 Delete from tReprotPackageTemplateAssignment table,
        //  2 Delete from tReportPackageTemplateElementType table,
        //  3 Delete from tReportPackageTemplateElementCategory table,
        //  4 Create records in tReprotPackageTemplateAssignment table,
        //  5 Create records in tReportPackageTemplateElementCategory table,
        //  6 Create records in tReportPackageTemplateElementElementType table

        // For new creation, do following steps
        //  1. Create on record in tReportPackageTempate,
        //  2. Create one records in  tReprotPackageTemplateAssignment,
        //  3. For AttachmentType and CertificationTypes, do below: 
        //      2.1 Create one record in tReportPackageTemplateElementCategory 
        //      2.2 Create records in table tReportPackageTemplateElementType
        /// </summary>
        /// <param name="rpt">The ReportPackageTemplateDto Object</param> 
        public void SaveReportPackageTemplate(ReportPackageTemplateDto rpt)
        {
            using (var transaction = _dbContext.BeginTransaction())
            {
                List<RuleViolation> validationIssues = new List<RuleViolation>();

                try
                {
                    var currentUserId = int.Parse(_httpContextService.GetClaimValue(CacheKey.UserProfileId));
                    var currentRegulatoryProgramId =
                        int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

                    //// Check report template name and effective date  
                    if (string.IsNullOrEmpty(rpt.Name.Trim()))
                    {
                        string message = "Name is required.";
                        validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                        throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                    }

                    if (rpt.EffectiveDateTimeUtc == DateTimeOffset.MinValue)
                    {
                        string message = "EffectiveDateTimeUtc is required.";
                        validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                        throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                    }

                    //// Check if Name and EffectiveDateTimeUtc combination is unique or not 
                    var testRpt = _dbContext.ReportPackageTempates
                        .FirstOrDefault(i => i.EffectiveDateTimeUtc == rpt.EffectiveDateTimeUtc &&
                                  i.Name == rpt.Name &&
                                  i.OrganizationRegulatoryProgramId == currentRegulatoryProgramId);

                    if (testRpt != null)
                    {
                        //// Creating a new report package template.
                        //// Or Updating the existing one, but testRpt is another one.
                        if (rpt.ReportPackageTemplateId.HasValue == false || testRpt.ReportPackageTemplateId != rpt.ReportPackageTemplateId.Value)
                        {
                            string message =
                                "A Template with that Name already exists for this Effective Date.  Select another name or effective date";
                            validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null,
                                errorMessage: message));
                            throw new RuleViolationException(message: "Validation errors",
                                validationIssues: validationIssues);
                        }
                    }

                    var reportPackageTemplate = _mapHelper.GetReportPackageTemplateFromReportPackageTemplateDto(rpt);

                    if (rpt.ReportPackageTemplateId.HasValue)
                    {
                        var currentReportPackageTempalte =
                            _dbContext.ReportPackageTempates.Single(i => i.ReportPackageTemplateId == rpt.ReportPackageTemplateId.Value);

                        //// Update current reportPackageTampate 
                        currentReportPackageTempalte.Name = rpt.Name;
                        currentReportPackageTempalte.Description = rpt.Description;
                        currentReportPackageTempalte.EffectiveDateTimeUtc = rpt.EffectiveDateTimeUtc;
                        currentReportPackageTempalte.RetirementDateTimeUtc = rpt.RetirementDateTimeUtc;
                        currentReportPackageTempalte.CtsEventTypeId = rpt.CtsEventTypeId;
                        currentReportPackageTempalte.OrganizationRegulatoryProgramId = currentRegulatoryProgramId;
                        currentReportPackageTempalte.IsActive = rpt.IsActive;
                        currentReportPackageTempalte.LastModificationDateTimeUtc = DateTimeOffset.UtcNow;
                        currentReportPackageTempalte.LastModifierUserId = currentUserId;

                        //  1 Delete from tReprotPackageTemplateAssignment table,
                        //  2 Delete from tReportPackageTemplateElementType table,
                        //  3 Delete from tReportPackageTemplateElementCategory table 
                        DeleteReportPackageChildrenObjects(reportPackageTemplate);
                    }
                    else
                    {
                        // 1 Create new in tReportPackageTempate table
                        reportPackageTemplate.OrganizationRegulatoryProgramId = currentRegulatoryProgramId;
                        reportPackageTemplate.IsActive = rpt.IsActive;
                        reportPackageTemplate.LastModificationDateTimeUtc = DateTimeOffset.UtcNow;
                        reportPackageTemplate.LastModifierUserId = currentUserId;

                        reportPackageTemplate = _dbContext.ReportPackageTempates.Add(reportPackageTemplate);
                    }

                    // 2 Create records in tReportTemplateAssignment table
                    foreach (var assignment in reportPackageTemplate.ReportPackageTemplateAssignments)
                    {
                        _dbContext.ReportPackageTemplateAssignments.Add(assignment);
                    }

                    _dbContext.SaveChanges();

                    // 3 AttachmentType   
                    if (rpt.AttachmentTypes != null && rpt.AttachmentTypes.Count > 0)
                    {
                        var attachmentTypes = rpt.AttachmentTypes.ToArray();
                        CreateReportPackageElementCatergoryType(attachmentTypes, reportPackageTemplate, false);
                    }

                    // 4 CertificationType  
                    if (rpt.CertificationTypes != null && rpt.CertificationTypes.Count > 0)
                    {
                        var certificationTypes = rpt.CertificationTypes.ToArray();
                        CreateReportPackageElementCatergoryType(certificationTypes, reportPackageTemplate, true);
                    }

                    _dbContext.SaveChanges();
                    transaction.Commit();

                    //transaction.Rollback();

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

                    _logger.Error("Error happens {0} ", string.Join("," + Environment.NewLine, errors));
                    throw;
                }
            }
        }

        public IEnumerable<CtsEventTypeDto> GetCtsEventTypes()
        {
            var ctsEventTypes = _dbContext.CtsEventTypes.ToList();
            return ctsEventTypes.Select(i => _mapHelper.GetCtsEventTypeDtoFromEventType(i));
        }

        public IEnumerable<ReportElementTypeDto> GetCertificationTypes()
        {
            return ReportElementTypeDtos(ReportElementCategoryName.Certifications.ToString());
        }

        public IEnumerable<ReportElementTypeDto> GetAttachmentTypes()
        {
            return ReportElementTypeDtos(ReportElementCategoryName.Attachments.ToString());
        }

        #region private section

        private IEnumerable<ReportElementTypeDto> ReportElementTypeDtos(string categoryName)
        {
            var currentRegulatoryProgramId =
                int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

            var reportElementTypes = _dbContext.ReportElementTypes.Where(
                    i => i.OrganizationRegulatoryProgramId == currentRegulatoryProgramId &&
                         i.ReportElementCategory.Name == categoryName
                )
                //.Select(_mapHelper.GetReportElementTypeDtoFromReportElementType)
                .ToList();

            var reportElementTypeDtos = new List<ReportElementTypeDto>();

            foreach (var rpt in reportElementTypes)
            {
                var reportElementTypeDto = _mapHelper.GetReportElementTypeDtoFromReportElementType(rpt);
                if (rpt.LastModifierUserId.HasValue)
                {
                    reportElementTypeDto.LastModificationDateTimeLocal = _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(rpt.LastModificationDateTimeUtc.Value.DateTime, _currentOrgRegProgramId);
                    var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == rpt.LastModifierUserId.Value);
                    reportElementTypeDto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
                }
            }

            return reportElementTypeDtos;
        }

        private void CreateReportPackageElementCatergoryType(ReportElementTypeDto[] reportElementTypeDtos,
            ReportPackageTemplate reportPackageTemplate, bool setOrder)
        {
            // Step 1, Save to tReportPackageTemplateElementCategory table
            var reportElementCategoryId = reportElementTypeDtos[0].ReportElementCategoryId;
            var rptec = new ReportPackageTemplateElementCategory
            {
                ReportPackageTemplateId = reportPackageTemplate.ReportPackageTemplateId,
                ReportElementCategoryId = reportElementCategoryId,
                SortOrder = setOrder
            };

            rptec = _dbContext.ReportPackageTemplateElementCategories.Add(rptec);
            // Save changes to get Id; 
            _dbContext.SaveChanges();
            var rptecId = rptec.ReportPackageTemplateElementCategoryId;

            foreach (var elementTypeDto in reportElementTypeDtos)
            {
                // Go Step 2,  Save to table tReportPackageTemplateElementType 
                if (elementTypeDto.ReportElementTypeId.HasValue == false)
                {
                    throw new Exception("Invalid ReportElementType");
                }

                var rptet = new ReportPackageTemplateElementType
                {
                    ReportPackageTemplateElementCategoryId = rptecId,
                    ReportElementTypeId = elementTypeDto.ReportElementTypeId.Value
                };

                _dbContext.ReportPackageTemplateElementTypes.Add(rptet);
                _dbContext.SaveChanges();
            }
        }

        private ReportPackageTemplateDto GetReportOneReportPackageTemplate(ReportPackageTemplate rpt)
        {
            var rptDto = _mapHelper.GetReportPackageTemplateDtoFromReportPackageTemplate(rpt);

            //1. set AttachmentTypes  
            var cat = rpt.ReportPackageTemplateElementCategories
                .FirstOrDefault(i => i.ReportElementCategory.Name == ReportElementCategoryName.Attachments.ToString());

            rptDto.AttachmentTypes = new List<Dto.ReportElementTypeDto>();
            if (cat != null)
            {
                var rets = GetReportElementType(cat);
                foreach (var ret in rets)
                {
                    var retDto = _mapHelper.GetReportElementTypeDtoFromReportElementType(ret);
                    if (ret.LastModifierUserId.HasValue)
                    {
                        retDto.LastModificationDateTimeLocal = _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(ret.LastModificationDateTimeUtc.Value.DateTime, _currentOrgRegProgramId);
                        var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == rpt.LastModifierUserId.Value);
                        retDto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
                    }
                    rptDto.AttachmentTypes.Add(retDto);
                }
            }

            //2. set certifications   
            cat = rpt.ReportPackageTemplateElementCategories
                .FirstOrDefault(
                    i => i.ReportElementCategory.Name == ReportElementCategoryName.Certifications.ToString());

            rptDto.CertificationTypes = new List<Dto.ReportElementTypeDto>();
            if (cat != null)
            {
                var rets = GetReportElementType(cat);

                //rptDto.CertificationTypes =
                //    rets.Select(i => _mapHelper.GetReportElementTypeDtoFromReportElementType(i)).Distinct().ToList();

                foreach (var ret in rets)
                {
                    var retDto = _mapHelper.GetReportElementTypeDtoFromReportElementType(ret);
                    if (ret.LastModifierUserId.HasValue)
                    {
                        retDto.LastModificationDateTimeLocal = _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(ret.LastModificationDateTimeUtc.Value.DateTime, _currentOrgRegProgramId);
                        var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == rpt.LastModifierUserId.Value);
                        retDto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
                    }

                    rptDto.CertificationTypes.Add(retDto);
                }
            }

            //3. set assingedIndustries  
            rptDto.ReportPackageTemplateAssignments = rptDto.ReportPackageTemplateAssignments;

            //// TODO ?
            //// Do I need to call the service to popute OrgRegProg    

            if (rpt.LastModifierUserId.HasValue)
            {
                rptDto.LastModifierUserDto = _userService.GetUserProfileById(rpt.LastModifierUserId.Value);
            }
            return rptDto;
        }

        private IEnumerable<ReportElementType> GetReportElementType(ReportPackageTemplateElementCategory cat)
        {
            var rptets = _dbContext.ReportPackageTemplateElementTypes.Where(
                    i =>
                        i.ReportPackageTemplateElementCategoryId ==
                        cat.ReportPackageTemplateElementCategoryId)
                .Distinct()
                .ToList();

            var rets = rptets.Select(i => i.ReportElementType).Distinct();
            foreach (var ret in rets)
            {
                ret.CtsEventType =
                    _dbContext.CtsEventTypes.FirstOrDefault(i => i.CtsEventTypeId == ret.CtsEventTypeId);
            }
            return rets;
        }

        private void DeleteReportPackageTemplateAssignments(
            IEnumerable<ReportPackageTemplateAssignment> reportPackageTemplateAssignments)
        {
            foreach (var assignment in reportPackageTemplateAssignments)
            {
                var temp = _dbContext.ReportPackageTemplateAssignments.Single(
                        i => i.ReportPackageTemplateAssignmentId == assignment.ReportPackageTemplateAssignmentId);

                if (temp != null)
                {
                    _dbContext.ReportPackageTemplateAssignments.Remove(temp);
                }
            }
        }

        private void DeleteReportPackageChildrenObjects(ReportPackageTemplate rpt)
        {
            DeleteReportPackageTemplateAssignments(rpt.ReportPackageTemplateAssignments);

            foreach (var rptec in rpt.ReportPackageTemplateElementCategories)
            {
                var rptetId = rptec.ReportPackageTemplateElementCategoryId;
                var rptets =
                    _dbContext.ReportPackageTemplateElementTypes.Where(
                        i => i.ReportPackageTemplateElementCategoryId == rptetId);

                foreach (var rptet in rptets)
                {
                    // Step 2.1
                    _dbContext.ReportPackageTemplateElementTypes.Remove(rptet);
                }

                // Step 2.2
                var temp =
                    _dbContext.ReportPackageTemplateElementCategories.Single(
                        i => i.ReportPackageTemplateElementCategoryId == rptec.ReportPackageTemplateElementCategoryId);

                _dbContext.ReportPackageTemplateElementCategories.Remove(temp);
            }
        }

        #endregion
    }
}