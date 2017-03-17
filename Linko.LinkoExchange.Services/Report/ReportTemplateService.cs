using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.User;
using NLog;

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

        public ReportTemplateService(
            LinkoExchangeContext dbContext,
            IHttpContextService httpContextService,
            UserService userService,
            IMapHelper mapHelper,
            ILogger logger)
        {
            _dbContext = dbContext;
            _httpContextService = httpContextService;
            _mapHelper = mapHelper;
            _logger = logger;
            _userService = userService;
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
                try
                {
                    var currentUserId = int.Parse(_httpContextService.GetClaimValue(CacheKey.UserProfileId));
                    var currentRegulatoryProgramId =
                        int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

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

                    // 3 AttachmentType   
                    var attachmentTypes = rpt.AttachmentTypes.ToArray();
                    CreateReportPackageElementCatergoryType(attachmentTypes, reportPackageTemplate, false);

                    // 4 CertificationType 
                    var certificationTypes = rpt.CertificationTypes.ToArray();
                    CreateReportPackageElementCatergoryType(certificationTypes, reportPackageTemplate, true);

                    _dbContext.SaveChanges();
                    transaction.Commit();
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
            return ReportElementTypeDtos(ReportElementCategoryName.Certification.ToString());
        }

        public IEnumerable<ReportElementTypeDto> GetAttachmentTypes()
        {
            return ReportElementTypeDtos(ReportElementCategoryName.Attachment.ToString());
        }

        #region private section

        private IEnumerable<ReportElementTypeDto> ReportElementTypeDtos(string categoryName)
        {
            var currentRegulatoryProgramId =
                int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

            var reprotElementTypes = _dbContext.ReportElementTypes.Where(
                    i => i.OrganizationRegulatoryProgramId == currentRegulatoryProgramId &&
                         i.ReportElementCategory.Name == categoryName
                )
                .Select(_mapHelper.GetReportElementTypeDtoFromReportElementType)
                .ToList();

            foreach (var rpt in reprotElementTypes)
            {
                if (rpt.LastModifierUserId.HasValue)
                {
                    rpt.LastModifierUser =
                        _mapHelper.GetUserDtoFromUserProfile(
                            _dbContext.Users.FirstOrDefault(i => i.UserProfileId == rpt.LastModifierUserId.Value));
                }
            }

            return reprotElementTypes;
        }

        private void CreateReportPackageElementCatergoryType(IEnumerable<ReportElementTypeDto> reportElementTypeDtos,
            ReportPackageTemplate reportPackageTemplate, bool setOrder)
        {
            foreach (var elementTypeDto in reportElementTypeDtos)
            {
                var reportElementCategoryId = elementTypeDto.ReportElementCategoryId;
                var rptec = new ReportPackageTemplateElementCategory
                {
                    ReportPackageTemplateId = reportPackageTemplate.ReportPackageTemplateId,
                    ReportElementCategoryId = reportElementCategoryId,
                    SortOrder = setOrder
                };

                rptec = _dbContext.ReportPackageTemplateElementCategories.Add(rptec);

                // Go Step 2,  Save to table tReportPackageTemplateElementType 
                var rptecId = rptec.ReportPackageTemplateElementCategoryId;

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
            }
        }

        private ReportPackageTemplateDto GetReportOneReportPackageTemplate(ReportPackageTemplate rpt)
        {
            var rptDto = _mapHelper.GetReportPackageTemplateDtoFromReportPackageTemplate(rpt);

            //1. set AttachmentTypes  
            var cat = rpt.ReportPackageTemplateElementCategories
                .FirstOrDefault(i => i.ReportElementCategory.Name == ReportElementCategoryName.Attachment.ToString());

            if (cat != null)
            {
                var rets = GetReportElementType(cat);

                rptDto.AttachmentTypes =
                    rets.Select(i => _mapHelper.GetReportElementTypeDtoFromReportElementType(i)).ToList();
            }

            //2. set certifications   
            cat = rpt.ReportPackageTemplateElementCategories
                .FirstOrDefault(
                    i => i.ReportElementCategory.Name == ReportElementCategoryName.Certification.ToString());

            if (cat != null)
            {
                var rets = GetReportElementType(cat);

                rptDto.CertificationTypes =
                    rets.Select(i => _mapHelper.GetReportElementTypeDtoFromReportElementType(i)).ToList();
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
                .ToList();

            var rets = rptets.Select(i => i.ReportElementType);
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
                _dbContext.ReportPackageTemplateAssignments.Remove(assignment);
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
                _dbContext.ReportPackageTemplateElementCategories.Remove(rptec);
            }
        }

        #endregion
    }
}