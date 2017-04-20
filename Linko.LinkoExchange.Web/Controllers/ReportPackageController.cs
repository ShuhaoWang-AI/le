using System;
using System.Linq;
using System.Web.Mvc;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.Unit;

namespace Linko.LinkoExchange.Web.Controllers
{
    /// <summary>
    ///     This is a testing controller,  dev who implement this controller should replace it.
    /// </summary>
    public class ReportPackageController:Controller
    {
        private readonly IHttpContextService _httpContextService;
        private readonly IReportPackageService _reportPackageService;
        private readonly IReportTemplateService _reportTemplateService;
        private readonly ISettingService _settingService;
        private readonly IUnitService _unitService;

        public ReportPackageController(
            IReportTemplateService reportTemplateService,
            ISettingService settingService,
            IHttpContextService httpContextService,
            IUnitService unitService,
            IReportPackageService reportPackageService)
        {
            if (reportTemplateService == null)
            {
                throw new NullReferenceException(message:"reportTemplateService");
            }

            _reportTemplateService = reportTemplateService;
            _settingService = settingService;
            _httpContextService = httpContextService;
            _unitService = unitService;
            _reportPackageService = reportPackageService;
        }

        // GET: ReprotPackage 
        public JsonResult Index()
        {
            var templatesList = _reportTemplateService.GetReportPackageTemplates().ToList();

            foreach (var rpt in templatesList)
            {
                rpt.Name += "_";
                rpt.Description += "_";
            }

            var tempate = _reportTemplateService.GetReportPackageTemplate(reportPackageTemplateId:1);

            return Json(data:templatesList, behavior:JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Testing Update
        /// </summary>
        /// <returns></returns>
        public JsonResult Update()
        {
            var templatesList = _reportTemplateService.GetReportPackageTemplates().ToList();

            foreach (var rpt in templatesList)
            {
                rpt.Name += "_";
                rpt.Description += "_";
                rpt.ShowSampleResults = true;

                _reportTemplateService.SaveReportPackageTemplate(rpt:rpt);
            }

            templatesList = _reportTemplateService.GetReportPackageTemplates().ToList();

            return Json(data:templatesList, behavior:JsonRequestBehavior.AllowGet);
        }

        public JsonResult AttachmentTypes()
        {
            var list = _reportTemplateService.GetAttachmentTypes();
            return Json(data:list, behavior:JsonRequestBehavior.AllowGet);
        }

        public JsonResult CertificationTypes()
        {
            var list = _reportTemplateService.GetCertificationTypes();
            return Json(data:list, behavior:JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetIndustryPPT_ResultQualifiersIndustryCanUse()
        {
            var currentRegulatoryProgramId =
                int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

            var settings = _settingService.GetSettingTemplateValue(settingType:SettingType.ResultQualifierValidValues);

            return Json(data:settings, behavior:JsonRequestBehavior.AllowGet);
        }

        public JsonResult update_ResultQualifiersIndustryCanUse()
        {
            var currentRegulatoryProgramId =
                int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

            var settingDto = new SettingDto();
            settingDto.TemplateName = SettingType.ResultQualifierValidValues;
            settingDto.Value = "GreaterThan,LessThan";
            settingDto.OrgTypeName = OrganizationTypeName.Authority;

            _settingService.CreateOrUpdateProgramSetting(orgRegProgId:currentRegulatoryProgramId, settingDto:settingDto);

            return Json(data:"Saving good", behavior:JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetIndustryPPT_CreateSampleNamesRule()
        {
            var currentRegulatoryProgramId =
                int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

            var settings = _settingService.GetSettingTemplateValue(settingType:SettingType.SampleNameCreationRule);

            return Json(data:settings, behavior:JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUnits()
        {
            var units = _unitService.GetFlowUnits();
            return Json(data:units, behavior:JsonRequestBehavior.AllowGet);
        }

        [Route(template:"submission/{reportPackageId:int}")]
        public ViewResult Submission(int reportPackageId)
        {
            _reportPackageService.SignAndSubmitReportPackage(reportPackageId:reportPackageId);
            return View();
        }
    }
}