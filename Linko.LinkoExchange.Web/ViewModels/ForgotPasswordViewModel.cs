using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Web.ViewModels
{

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}