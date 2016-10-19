using System.Collections.Generic;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Web.ViewModels;
using System.Threading.Tasks;
using System.Web.Mvc;
using Linko.LinkoExchange.Services.Email;

namespace Linko.LinkoExchange.Web.Controllers
{
    public class AccountController : Controller
    { 
        //private IAuditLogService _auditLogService;
        private readonly IAuthenticationService _authenticateService;
        private readonly IEmailService _emailService;

        public AccountController(IAuthenticationService authenticateService, IEmailService emailService)
        {
            _authenticateService = authenticateService;
            _emailService = emailService;
        }

        // GET: Account
        public async Task<ActionResult> Index()
        { 
//            var contentReplacements = new Dictionary<string, string>();
//            contentReplacements.Add("organizationName", "Green Vally Plant");
//            contentReplacements.Add("authorityName", "Grand Rapids");
//            contentReplacements.Add("userName", "Shuhao Wang");
//            contentReplacements.Add("addressLine1", "1055 Pender Street");
//            contentReplacements.Add("cityName", "Vancouver");
//            contentReplacements.Add("stateName", "BC");
//
//            var receivers = new List<string> { "shuhao.wang@watertrax.com" };  
//            _emailService.SendEmail(receivers, Linko.LinkoExchange.Core.Enum.EmailType.Signature_SignatoryGranted, contentReplacements); 

            return View ();
        }

        [AllowAnonymous]
        public ActionResult SignIn(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> SignIn(SignInViewModel model)
        {
            var email = model.UserName;
            var password = model.Password;
            var signInResult = await _authenticateService.SignInByUserName(email, password, true);

            // TODO to test set addition information  
            var testClaims = new Dictionary<string, string>();
            testClaims.Add("currentAuthorityId", "123435");
            testClaims.Add("currentPrograId", "67890");
            testClaims.Add("currentIndustryId", "industry_001");

            _authenticateService.SetCurrentUserClaims(testClaims);
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