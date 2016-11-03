using System.Web.Mvc;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Web.ViewModels.Industry;
using NLog;

namespace Linko.LinkoExchange.Web.Controllers
{
    [RoutePrefix("Industry")]
    public class IndustryController : Controller
    {
        #region constructor
        
        private readonly IOrganizationService _organizationService;
        private readonly ISessionCache _sessionCache;
        private readonly ILogger _logger;

        public IndustryController(IOrganizationService organizationService, ISessionCache sessionCache, ILogger logger)
        {
            _organizationService = organizationService;
            _sessionCache = sessionCache;
            _logger = logger;
        }

        #endregion

        #region default action

        // GET: Industry
        public ActionResult Index()
        {
            return RedirectToAction(actionName: "Settings", controllerName: "Industry");
        }
        #endregion

        #region Show Industry Details

        // GET: /Industry/Settings
        [Route("Settings")]
        public ActionResult Settings()
        {
            int currentOrganizationRegulatoryProgramId = int.Parse(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var industry = _organizationService.GetOrganizationRegulatoryProgram(currentOrganizationRegulatoryProgramId);

            var viewModel = new IndustryViewModel
            {
                ID = industry.OrganizationRegulatoryProgramId,
                IndustryNo = industry.OrganizationDto.OrganizationId,
                IndustryName = industry.OrganizationDto.OrganizationName,
                AddressLine1 = industry.OrganizationDto.AddressLine1,
                AddressLine2 = industry.OrganizationDto.AddressLine2,
                CityName = industry.OrganizationDto.CityName,
                State = industry.OrganizationDto.State,
                ZipCode = industry.OrganizationDto.ZipCode,
                PhoneNumber = industry.OrganizationDto.PhoneNumber,
                PhoneExt = industry.OrganizationDto.PhoneExt,
                FaxNumber = industry.OrganizationDto.FaxNumber,
                WebsiteUrl = industry.OrganizationDto.WebsiteURL
            };

            return View(viewModel);
        }

        // POST: /Industry/Settings
        [AcceptVerbs(HttpVerbs.Post)]
        [Route("Settings")]
        public ActionResult Settings(IndustryViewModel model)
        {
            return View(model);
        }
        #endregion
    }
}