using System;
using System.Collections.Generic;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using NLog;

namespace Linko.LinkoExchange.Services.Report
{
    public class ReportTemplateService : IReportTemplateService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContextService;
        private readonly IAttachmentService _attachmentService;
        private readonly IMapHelper _mapHelper;
        private readonly ILogger _logger;


        public ReportTemplateService(
            LinkoExchangeContext dbContext,
            IHttpContextService httpContextService,
            IAttachmentService attachmentService,
            IMapHelper mapHelper,
            ILogger logger)
        {
            _dbContext = dbContext;
            _httpContextService = httpContextService;
            _attachmentService = attachmentService;
            _mapHelper = mapHelper;
            _logger = logger;
        }

        public void DeleteReportPackageTemplate(int reportPackageTemplateId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CtsEventTypeDto> GetCtsEventTypes()
        {
            throw new NotImplementedException();
        }

        public ReportPackageTemplateDto GetReportPackageTemplate(int reportPackageTemplateId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ReportPackageTemplateDto> GetReportPackageTemplates()
        {
            //TODO get  reportPackageTempates 
            var rpts = new List<ReportPackageTemplate>();

            var rptDtos = new List<ReportPackageTemplateDto>();
            foreach (var rpt in rpts)
            {
                var rptDto = _mapHelper.GetReportPackageTemplateDtoFromReportPackageTemplate(rpt);

                // TODO 
                //1. set AttachmentTypes  
                var atts = rpt.ReportPackageTemplateElementCategories
                    .Where(i => i.ReportElementCategory.Name == ReportElementCategoryName.Attachment.ToString())
                    .SelectMany(i => i.ReportPackageTemplateElementTypes).Distinct().ToList();

                var attachments = new List<AttachmentTypeDto>();
                foreach (var att in atts)
                {
                    attachments.Add(_mapHelper.GetAttachmentTypeDtoFromReportElementType(att.ReportElementType));
                }
                rptDto.AttachmentTypes = attachments;

                //2. set certifications  
                var certs = rpt.ReportPackageTemplateElementCategories
                   .Where(i => i.ReportElementCategory.Name == ReportElementCategoryName.Certification.ToString())
                   .SelectMany(i => i.ReportPackageTemplateElementTypes).Distinct().ToList();

                rptDto.CertificationTypes = certs.Select(cert => _mapHelper.GetCertificationTypeDtoFromReportElementType(cert.ReportElementType)).ToList();

                //3. set assingedIndustries  
                rptDto.ReportPackageTemplateAssignments = rptDto.ReportPackageTemplateAssignments;

                /// TODO  
                /// to can the service to popute OrgRegProg  
                /// 
                rptDtos.Add(rptDto);
            }

            return rptDtos;
        }

        public void SaveReportPackageTemplate(ReportPackageTemplateDto rpt)
        {

            //    if (rpt.Id.HasValue)
            //    {
            //Update existing
            //        certificationTypeToPersist = _dbContext.ReportElementTypes.Single(c => c.ReportElementTypeId == certType.CertificationTypeID);
            //        certificationTypeToPersist = _mapHelper.GetReportElementTypeFromCertificationTypeDto(certType, certificationTypeToPersist);
            //    }
            //    else
            //    {
            //Create new
            //        var reportPackageTemplate = _mapHelper.GetReportPackageTemplateFromReportPackageTemplateDto(rpt);
            //        _dbContext.ReportPackageTempates.Add(reportPackageTemplate);

            // Step 1
            // Create records in table tReportPackageTemplateElementCatory 
            // Fill in all catergories that belong to the reportPackageTemplate   

            // category 1, attachment

            //        var

            //        var attachmentDto = rpt.AttachmentTypes.ToList();
            //        var category = new ReportPackageTemplateElementCategory
            //        {
            //            ReportElementCategoryId = attachmentDto.,
            //            ReportPackageTemplateId = reportPackageTemplate.ReportPackageTemplateId
            //        };


            //        var category = rpt.ReportPackageTemplateElementCategories.OrderBy(i => i.SortOrder);
            //        foreach (var cat in categoreis)
            //        {
            //            var category = new ReportPackageTemplateElementCategory
            //            {
            //                ReportElementCategoryId = cat.ReportPackageTemplateElementCategoryId,
            //                ReportPackageTemplateId = reportPackageTemplate.ReportPackageTemplateId
            //            };

            //add into table 
            //            _dbContext.ReportPackageTemplateElementCategories.Add(category);


            // Step 2 
            // Create records in table tReportPackageTempalteElememtType 
            // To fill in reportElementType data that belong to each category 
            // 

            //        }

            //        var rptts = reportPackageTemplate.ReportPackageTemplateElementCategories.Select(i => i.ReportElementCategory.t)

            // Create records in table :  tReportPackageTemplateType 
            //}
            //    _dbContext.SaveChanges();

        }
    }
}