using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Web.ViewModels.ReportPackage;
using NLog;

namespace Linko.LinkoExchange.Web.Controllers
{
    [RoutePrefix(prefix:"ReportPackage")]
    public class ReportPackageController:Controller
    {
        #region default action

        // GET: ReprotPackage
        public ActionResult Index()
        {
            return RedirectToAction(actionName:"ReportPackages", controllerName:"ReportPackage", routeValues:new {reportStatus = ReportStatusName.Draft});
        }

        #endregion

        #region constructor

        private readonly IReportPackageService _reportPackageService;
        private readonly ILogger _logger;

        public ReportPackageController(IReportPackageService reportPackageService, ILogger logger)
        {
            _reportPackageService = reportPackageService;
            _logger = logger;
        }

        #endregion

        #region Show Report Package List

        // GET: /ReportPackage/ReportPackages
        [Route(template:"ReportPackages/{reportStatus}")]
        public ActionResult ReportPackages(ReportStatusName reportStatus)
        {
            ViewBag.ReportStatusName = reportStatus;
            return View();
        }

        public ActionResult ReportPackages_Read([DataSourceRequest] DataSourceRequest request, ReportStatusName reportStatus)
        {
            var dtos = _reportPackageService.GetReportPackagesByStatusName(reportStatusName:reportStatus, isAuthorityViewing:false);

            var viewModels = dtos.Select(vm => new ReportPackageViewModel
                                               {
                                                   Id = vm.ReportPackageId,
                                                   LastModificationDateTimeLocal = vm.LastModificationDateTimeLocal,
                                                   LastModifierUserName = vm.LastModifierFullName,
                                                   LastSentDateTimeLocal = vm.LastSentDateTimeLocal,
                                                   Name = vm.Name,
                                                   OrganizationName = vm.OrganizationName,
                                                   OrganizationReferenceNumber = vm.OrganizationReferenceNumber,
                                                   PeriodEndDateTimeLocal = vm.PeriodEndDateTimeLocal,
                                                   PeriodStartDateTimeLocal = vm.PeriodStartDateTimeLocal,
                                                   RepudiationDateTimeLocal = vm.RepudiationDateTimeLocal,
                                                   RepudiatorFirstName = vm.RepudiatorFirstName,
                                                   RepudiatorLastName = vm.RepudiatorLastName,
                                                   Status = vm.ReportStatusName,
                                                   SubmissionDateTimeLocal = vm.SubmissionDateTimeLocal,
                                                   SubmitterFirstName = vm.SubmitterFirstName,
                                                   SubmitterLastName = vm.SubmitterLastName
                                               });

            var result = viewModels.ToDataSourceResult(request:request, selector:vm => new
                                                                                       {
                                                                                           vm.Id,
                                                                                           vm.LastModificationDateTimeLocal,
                                                                                           vm.LastModifierUserName,
                                                                                           vm.LastSentDateTimeLocal,
                                                                                           vm.Name,
                                                                                           vm.OrganizationName,
                                                                                           vm.OrganizationReferenceNumber,
                                                                                           vm.PeriodEndDateTimeLocal,
                                                                                           vm.PeriodStartDateTimeLocal,
                                                                                           vm.RepudiationDateTimeLocal,
                                                                                           vm.Repudiator,
                                                                                           vm.Status,
                                                                                           vm.SubmissionDateTimeLocal,
                                                                                           vm.Submitter
                                                                                       });

            return Json(data:result);
        }

        #endregion
    }
}