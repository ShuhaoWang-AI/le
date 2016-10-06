using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Web.ViewModels
{
    public class UserInfoViewModel
    {
        [Required]
        [Display(Name = "UserDto Name")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Fist Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}