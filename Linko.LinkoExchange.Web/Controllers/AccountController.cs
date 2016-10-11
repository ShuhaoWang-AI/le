using System.Collections.Generic;
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

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous] 
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {

                var result = await _authenticateService.RequestResetPassword(model.Email);
                if (result.Success)
                {
                    return RedirectToAction("", "");
                }
                else
                {
                    // redirect to show error of forget password
                    System.Console.WriteLine("Reset password error");
                    System.Console.WriteLine(result.Errors.ToString());

                    RedirectToLocal("error");
                } 
            }

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return string.IsNullOrWhiteSpace(code)  ? View("ErrorPage") : View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
             
            var result = await _authenticateService.ResetPasswordAsync(model.Email, model.Code, model.Password);

            if (result.Success)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            AddErrors(result.Errors); 

            return View();
        }

        private void AddErrors(IEnumerable<string> errors)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError("", error);
            }
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