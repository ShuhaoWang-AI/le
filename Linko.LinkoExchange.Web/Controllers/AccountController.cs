using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Web.ViewModels;
using System.Threading.Tasks;
using System.Web.Mvc;


namespace Linko.LinkoExchange.Web.Controllers
{
    public class AccountController : Controller
    { 
        private IAuditLogService _auditLogService;
        private readonly IAuthenticationService _authenticateService; 

        public AccountController(IAuthenticationService authenticateService)
        {
            _authenticateService = authenticateService;    
        }

        // GET: Account
        public async Task<ActionResult> Index()
        {
            return View ();
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            var email = model.Email;
            var password = model.Password;
            var signInResult = await _authenticateService.SignInByUserName(email, password, true);  
             
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Register(RegisterViewModel model, string registrationToken)
        {
            var userInfo = new UserDto
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Password = model.Password,
                UserName = model.UserName
            };

            var newUser = await _authenticateService.CreateUserAsync(userInfo, registrationToken);

            return View();
        }
        public ActionResult SignOff()
        {
            _authenticateService.SignOff ();
            return View ();
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}