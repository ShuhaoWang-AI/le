using System;
using System.Linq;
using System.Web.Mvc;
using Linko.LinkoExchange.Services.Authentication;

namespace Linko.LinkoExchange.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly IAuthenticationService _authenticateService;

        public UserController(IAuthenticationService authenticateService)
        {
            if(authenticateService == null) throw  new ArgumentNullException("authenticateService");
            _authenticateService = authenticateService;
        }

        // GET: UserDto
        public ActionResult Index()
        {
            // TODO: to test get claims 
            var claims = _authenticateService.GetClaims();
            var organization = claims?.FirstOrDefault(i => i.Type == "OrganizationName");
            if (organization != null)
            {
                ViewBag.organizationName = organization.Value;
            }

            return View(); 
        }
    }
}