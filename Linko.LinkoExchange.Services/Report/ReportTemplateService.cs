using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Linko.LinkoExchange.Services
{
    public class ReportTemplateService : IReportTemplateService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContextService;
        private readonly IAttachmentService _attachmentService;
        private readonly IMapHelper _mapHelper;


        public ReportTemplateService(
            LinkoExchangeContext dbContext,
            IHttpContextService httpContextService,
            IAttachmentService attachmentService,
            IMapHelper mapHelper)
        {
            _dbContext = dbContext;
            _httpContextService = httpContextService;
            _attachmentService = attachmentService;
            _mapHelper = mapHelper;
        }

        public void deleteReportPackageTemplate(int reportPackageTemplateId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CtsEventTypeDto> GetCTSEventTypes()
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
            var rptDtos = new List<Dto.ReportPackageTemplateDto>();
            foreach (var rpt in rpts)
            {
                var rptDto = _mapHelper.GetReportPackageTemplateDtoFromReportPackageTemplate(rpt);

                // TODO 
                //1. set Attachments 
                rptDto.Attachments = rpt.ReportPackageTemplateElementCategories
                    .Where(i => i.ReportElementCategory.Name == Core.Enum.ReportElementCategory.Attachment.ToString())
                    .Select(i => _mapHelper.GetReportElementCategoryDtoFromReportElementCategory(i)).ToList();


                //2. set certifications 
                rptDto.Certifications = rpt.ReportPackageTemplateElementCategories
                   .Where(i => i.ReportElementCategory.Name == Core.Enum.ReportElementCategory.Certification.ToString())
                   .Select(i => _mapHelper.GetReportElementCategoryDtoFromReportElementCategory(i)).ToList();

                //3. set assingedIndustries 
                var assinedIndustries = rpt.ReportPackageTemplateAssignments.Select(i => i.OrganizationRegulatoryProgram);
                rptDto.AssignedIndustries = rpt.ReportPackageTemplateAssignments.Select(i => i.OrganizationRegulatoryProgram)
                        .Select(i => _mapHelper.GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(i)).ToList();

                rptDtos.Add(rptDto);
            }

            return rptDtos;
        }

        public void SaveReportPackageTemplate(Dto.ReportPackageTemplateDto rpt)
        {
            throw new NotImplementedException();
        }
    }
}