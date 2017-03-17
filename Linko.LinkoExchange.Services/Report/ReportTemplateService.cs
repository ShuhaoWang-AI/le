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

        private readonly int _orgRegProgramId;

        public ReportTemplateService(
            LinkoExchangeContext dbContext,
            IHttpContextService httpContextService,
            //IReportElementService reportElementService,
            UserService userService,
            IMapHelper mapHelper,
            ILogger logger)
        {
            _dbContext = dbContext;
            _httpContextService = httpContextService;
            //_reportElementService = reportElementService;
            _mapHelper = mapHelper;
            _logger = logger;
            _userService = userService;
            _orgRegProgramId = int.Parse(httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
        }

        public void DeleteReportPackageTemplate(int reportPackageTemplateId)
        {
            //Delete data from other tables before delete from  ReportPackageTempates 
            //// 1. Delete from tReportPackageTemplateAssignment
            ///  2. Delete from tReportPackageTemplateElementCategory 
            ///      2.1  Delete from  tReportPackageTemplateElementType table
            ///      2.2  Delete from  tReportPackageTemplateElementCategory table 
            ///  3.  Delete from tReportPackageTempates 

            var rpt =
                _dbContext.ReportPackageTempates.FirstOrDefault(
                    i => i.ReportPackageTemplateId == reportPackageTemplateId);
            if (rpt == null)
            {
                return;
            }

            foreach (var rptec in rpt.ReportPackageTemplateElementCategories)
            {
                var rptetId = rptec.ReportPackageTemplateElementCategoryId;
                var rptets =
                    _dbContext.ReportPackageTemplateElementTypes.Where(
                        i => i.ReportPackageTemplateElementCategoryId == rptetId);

                foreach (var rptet in rptets)
                {
                    _dbContext.ReportPackageTemplateElementTypes.Remove(rptet);
                }

                _dbContext.ReportPackageTemplateElementCategories.Remove(rptec);
            }

            var assignments =
                _dbContext.ReportPackageTemplateAssignments.Where(
                    i => i.ReportPackageTemplateId == reportPackageTemplateId);
            foreach (var assignment in assignments)
            {
                _dbContext.ReportPackageTemplateAssignments.Remove(assignment);
            }

            _dbContext.SaveChanges();

        }

        public IEnumerable<CtsEventTypeDto> GetCtsEventTypes()
        {
            var ctsEventTypes = _dbContext.CtsEventTypes.ToList();
            return ctsEventTypes.Select(i => _mapHelper.GetCtsEventTypeDtoFromEventType(i));
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
            var rpts =
                _dbContext.ReportPackageTempates.Where(i => i.OrganizationRegulatoryProgramId == _orgRegProgramId)
                    .Include(i => i.ReportPackageTemplateElementCategories)
                    .ToArray();

            return rpts.Select(GetReportOneReportPackageTemplate).ToList();
        }

        public void SaveReportPackageTemplate(ReportPackageTemplateDto rpt)
        {

            if (rpt.ReportPackageTemplateId.HasValue)
            {
                //   Update existing
                //certificationTypeToPersist = _dbContext.ReportElementTypes.Single(c => c.ReportElementTypeId == certType.CertificationTypeID);
                //certificationTypeToPersist = _mapHelper.GetReportElementTypeFromCertificationTypeDto(certType, certificationTypeToPersist);
            }
            else
            {
                //   Create new
                var reportPackageTemplate = _mapHelper.GetReportPackageTemplateFromReportPackageTemplateDto(rpt);
                _dbContext.ReportPackageTempates.Add(reportPackageTemplate);

                // Step 1
                //Create records in table tReportPackageTemplateElementCatory
                //Fill in all catergories that belong to the reportPackageTemplate

                // TODO:  Add other fields that can be convert here 
                // 1. AttachmentType collections 
                var attachmentTypes = rpt.AttachmentTypes.ToArray();
                foreach (var reportElementTypeDto in attachmentTypes)
                {
                    var reportElementTypeId = reportElementTypeDto.ReportElementTypeId;
                    var reportElementCategoryId = reportElementTypeDto.ReportElementCategoryId;

                    //TODO 
                    var rptec = new ReportPackageTemplateElementCategory
                    {
                        ReportPackageTemplateId = reportElementTypeId.Value,
                        ReportElementCategoryId = reportElementCategoryId
                    };

                    _dbContext.ReportPackageTemplateElementCategories.Add(rptec);

                    // Go Step 2,  save to table ReportPackageTemplateElementType 
                    var rptecId = rptec.ReportPackageTemplateElementCategoryId;

                    var rptet = new ReportPackageTemplateElementType
                    {
                        ReportPackageTemplateElementCategoryId = rptecId,
                        ReportElementTypeId = reportElementTypeDto.ReportElementTypeId.Value
                    };

                    _dbContext.ReportPackageTemplateElementTypes.Add(rptet);
                }


                //var category = rpt.ReportPackageTemplateElementCategories.OrderBy(i => i.SortOrder);
                //foreach (var cat in categoreis)
                //{
                //    var category = new ReportPackageTemplateElementCategory
                //    {
                //        ReportElementCategoryId = cat.ReportPackageTemplateElementCategoryId,
                //        ReportPackageTemplateId = reportPackageTemplate.ReportPackageTemplateId
                //    };

                // add into table
                //    _dbContext.ReportPackageTemplateElementCategories.Add(category);


                //       Step 2
                //Create records in table tReportPackageTempalteElememtType
                //To fill in reportElementType data that belong to each category



                //}

                //var rptts = reportPackageTemplate.ReportPackageTemplateElementCategories.Select(i => i.ReportElementCategory.t)


            }
            _dbContext.SaveChanges();
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

            //  rptDto.CertificationTypes = certs.Select(cert => _mapHelper.GetReportElementTypeDtoFromReportElementType(cert.ReportElementType)).ToList();

            //3. set assingedIndustries  
            rptDto.ReportPackageTemplateAssignments = rptDto.ReportPackageTemplateAssignments;

            //// TODO  
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

    }
}