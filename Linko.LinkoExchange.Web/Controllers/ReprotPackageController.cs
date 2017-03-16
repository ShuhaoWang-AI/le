using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Linko.LinkoExchange.Services.Report;

namespace Linko.LinkoExchange.Web.Controllers
{
    public class ReprotPackageController : Controller
    {
        private IReportTemplateService _reportTemplateService;
        public ReprotPackageController(IReportTemplateService reportTemplateService)
        {
            if (reportTemplateService == null)
            {
                throw new NullReferenceException("reportTemplateService");
            }

            _reportTemplateService = reportTemplateService;
        }
        // GET: ReprotPackage 
        public ActionResult Index()
        {
            var templates = _reportTemplateService.GetReportPackageTemplates();


            return View();
        }
    }
}