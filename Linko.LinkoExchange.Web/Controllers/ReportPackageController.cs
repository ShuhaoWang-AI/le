using System;
using System.Linq;
using System.Web.Mvc;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.Unit;

namespace Linko.LinkoExchange.Web.Controllers
{
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
    }
}