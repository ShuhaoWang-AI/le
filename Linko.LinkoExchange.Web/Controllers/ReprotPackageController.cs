using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.Settings;

namespace Linko.LinkoExchange.Web.Controllers
{

    /// <summary>
    /// This is a testing controller,  dev who implement this controller should replace it. 
    /// </summary>
    public class ReprotPackageController : Controller
    {
        private readonly IReportTemplateService _reportTemplateService;
        private readonly ISettingService _settingService;
        private readonly IHttpContextService _httpContextService;

        public ReprotPackageController(
            IReportTemplateService reportTemplateService,
            ISettingService settingService,
            IHttpContextService httpContextService)
        {
            if (reportTemplateService == null)
            {
                throw new NullReferenceException("reportTemplateService");
            }

            _reportTemplateService = reportTemplateService;
            _settingService = settingService;
            _httpContextService = httpContextService;
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

            var tempate = _reportTemplateService.GetReportPackageTemplate(1);

            return Json(templatesList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Testing Update
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

                _reportTemplateService.SaveReportPackageTemplate(rpt);
            }

            templatesList = _reportTemplateService.GetReportPackageTemplates().ToList();

            return Json(templatesList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AttachmentTypes()
        {
            var list = _reportTemplateService.GetAttachmentTypes();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CertificationTypes()
        {
            var list = _reportTemplateService.GetCertificationTypes();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetIndustryPPT_ResultQualifiersIndustryCanUse()
        {
            var currentRegulatoryProgramId =
                      int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

            var settings = _settingService.GetSettingTemplateValue(SettingType.ResultQualifierValidValues);

            return Json(settings, JsonRequestBehavior.AllowGet);
        }

        public JsonResult update_ResultQualifiersIndustryCanUse()
        {
            var currentRegulatoryProgramId =
                      int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

            var settingDto = new SettingDto();
            settingDto.TemplateName = SettingType.ResultQualifierValidValues;
            settingDto.Value = "GreaterThan,LessThan";
            settingDto.OrgTypeName = OrganizationTypeName.Authority;

            _settingService.CreateOrUpdateProgramSetting(currentRegulatoryProgramId, settingDto);

            return Json("Saving good", JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetIndustryPPT_CreateSampleNamesRule()
        {
            var currentRegulatoryProgramId =
                      int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

            var settings = _settingService.GetSettingTemplateValue(SettingType.SampleNameCreationRule);

            return Json(settings, JsonRequestBehavior.AllowGet);
        }

    }
}