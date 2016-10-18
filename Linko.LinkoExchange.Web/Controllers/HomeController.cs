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
                return RedirectToAction(actionName: "Index", controllerName: "User"); // ToDo: change with correct default controller
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Account");
            }
        }
    }
}