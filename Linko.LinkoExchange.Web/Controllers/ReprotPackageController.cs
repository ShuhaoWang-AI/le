using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Linko.LinkoExchange.Services.Report;

namespace Linko.LinkoExchange.Web.Controllers
{
    public class ReprotPackageController : Controller
    {
        private readonly IReportTemplateService _reportTemplateService;
        public ReprotPackageController(IReportTemplateService reportTemplateService)
        {
            if (reportTemplateService == null)
            {
                throw new NullReferenceException("reportTemplateService");
            }

            _reportTemplateService = reportTemplateService;
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
            }
            _reportTemplateService.SaveReportPackageTemplate(templatesList[0]);

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
    }
}