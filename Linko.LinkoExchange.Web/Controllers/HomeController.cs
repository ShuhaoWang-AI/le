using System.Web.Mvc;

namespace Linko.LinkoExchange.Web.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                //TODO: Need service to know current portal
                // if current portal is Authority
                return RedirectToAction(actionName: "Index", controllerName: "Authority"); 
                // if current portal is Industry
                return RedirectToAction(actionName: "Index", controllerName: "Industry"); 
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Account");
            }
        }
    }
}