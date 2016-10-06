using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Linko.LinkoExchange.Web.Controllers
{
    public class UserController : Controller
    {
        // GET: UserDto
        public ActionResult Index()
        {
            // TODO: to test get claims 

            var prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            return View(); 
        }
    }
}