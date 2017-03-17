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
            var list = _reportTemplateService.GetReportPackageTemplates();
            return Json(list, JsonRequestBehavior.AllowGet);
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