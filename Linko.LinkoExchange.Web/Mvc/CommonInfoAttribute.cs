using System.Linq;
using System.Web.Mvc;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.User;

namespace Linko.LinkoExchange.Web.Mvc
{
    public class CommonInfoAttribute : ActionFilterAttribute
    {
        #region constructor

        private readonly IHttpContextService _httpContextService;
        private readonly IUserService _userService;
        private readonly IReportPackageService _reportPackageService;
        public CommonInfoAttribute(IHttpContextService httpContextService,IUserService userService, IReportPackageService reportPackageService)
        {
            _httpContextService = httpContextService;
            _userService = userService;
            _reportPackageService = reportPackageService;
        }

        #endregion


        #region default action
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                var portalName = _httpContextService.GetClaimValue(claimType:CacheKey.PortalName);
                filterContext.Controller.ViewBag.PortalName = string.IsNullOrWhiteSpace(value:portalName) ? "" : portalName;
                filterContext.Controller.ViewBag.OrganizationName = string.IsNullOrWhiteSpace(value:portalName) ? "" : _httpContextService.GetClaimValue(claimType:CacheKey.OrganizationName);
                filterContext.Controller.ViewBag.UserName = _httpContextService.GetClaimValue(claimType:CacheKey.UserName);
                filterContext.Controller.ViewBag.UserRole = _httpContextService.GetClaimValue(claimType:CacheKey.UserRole);
                if (!string.IsNullOrWhiteSpace(value:portalName))
                {
                    var currentOrganizationRegulatoryProgramId = int.Parse( s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
                    filterContext.Controller.ViewBag.PendingRegistrationProgramUsersCount = _userService.GetPendingRegistrationProgramUsersCount(orgRegProgamId:currentOrganizationRegulatoryProgramId);
                    var reportPackageStatusCounts = _reportPackageService.GetReportPackageStatusCounts();
                    filterContext.Controller.ViewBag.ReportPackageCount_Draft = reportPackageStatusCounts.SingleOrDefault(c => c.Status == ReportStatusName.Draft)?.Count;
                    filterContext.Controller.ViewBag.ReportPackageCount_ReadyToSubmit = reportPackageStatusCounts.SingleOrDefault(c => c.Status == ReportStatusName.ReadyToSubmit)?.Count;
                    filterContext.Controller.ViewBag.ReportPackageCount_SubmittedPendingReview = reportPackageStatusCounts.SingleOrDefault(c => c.Status == ReportStatusName.SubmittedPendingReview)?.Count;
                    filterContext.Controller.ViewBag.ReportPackageCount_RepudiatedPendingReview = reportPackageStatusCounts.SingleOrDefault(c => c.Status == ReportStatusName.RepudiatedPendingReview)?.Count;
                }
            }
            else
            {
                filterContext.Controller.ViewBag.PortalName = "";
                filterContext.Controller.ViewBag.OrganizationName = "";
                filterContext.Controller.ViewBag.UserName = "";
                filterContext.Controller.ViewBag.UserRole = "";
            }
            base.OnResultExecuting(filterContext:filterContext);
        }
        #endregion
    }
}