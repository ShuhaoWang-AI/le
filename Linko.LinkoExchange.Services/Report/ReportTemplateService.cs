using System;
using System.Collections.Generic;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using NLog;

namespace Linko.LinkoExchange.Services.Report
{
    public class ReportTemplateService : IReportTemplateService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContextService;
        private readonly IReportElementService _reportElementService;
        private readonly IMapHelper _mapHelper;
        private readonly ILogger _logger;

        private readonly int _orgRegProgramId;

        public ReportTemplateService(
            LinkoExchangeContext dbContext,
            IHttpContextService httpContextService,
            IReportElementService reportElementService,
            IMapHelper mapHelper,
            ILogger logger)
        {
            _dbContext = dbContext;
            _httpContextService = httpContextService;
            _reportElementService = reportElementService;
            _mapHelper = mapHelper;
            _logger = logger;

            _orgRegProgramId = int.Parse(httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
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
            var rpts = _dbContext.ReportPackageTempates.Where(i => i.OrganizationRegulatoryProgramId == _orgRegProgramId).ToArray();

            var rptDtos = new List<ReportPackageTemplateDto>();
            foreach (var rpt in rpts)
            {
                var rptDto = _mapHelper.GetReportPackageTemplateDtoFromReportPackageTemplate(rpt);

                //1. set AttachmentTypes  
                var atts = rpt.ReportPackageTemplateElementCategories
                    .Where(i => i.ReportElementCategory.Name == ReportElementCategoryName.Attachment.ToString())
                    .SelectMany(i => i.ReportPackageTemplateElementTypes).Distinct().ToList();

                var attachments = new List<ReportElementTypeDto>();
                foreach (var att in atts)
                {
                    attachments.Add(_mapHelper.GetReportElementTypeDtoFromReportElementType(att.ReportElementType));
                }
                rptDto.AttachmentTypes = attachments;

                //2. set certifications  
                var certs = rpt.ReportPackageTemplateElementCategories
                   .Where(i => i.ReportElementCategory.Name == ReportElementCategoryName.Certification.ToString())
                   .SelectMany(i => i.ReportPackageTemplateElementTypes).Distinct().ToList();

                rptDto.CertificationTypes = certs.Select(cert => _mapHelper.GetReportElementTypeDtoFromReportElementType(cert.ReportElementType)).ToList();

                //3. set assingedIndustries  
                rptDto.ReportPackageTemplateAssignments = rptDto.ReportPackageTemplateAssignments;

                /// TODO  
                /// Do I need to call the service to popute OrgRegProg   
                /// 
                rptDtos.Add(rptDto);
            }

            return rptDtos;
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
                    var reportElementTypeId = reportElementTypeDto.ReportElementTypeID;
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
                        ReportElementTypeId = reportElementTypeDto.ReportElementTypeID.Value
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
    }
}