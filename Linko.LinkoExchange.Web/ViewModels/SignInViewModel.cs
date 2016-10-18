using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Web.ViewModels
{
    public class SignInViewModel
    {
        [Required]
        [Display(Name = "UserName", ResourceType = typeof(Core.Resources.Label))]
        [EmailAddress]
        public string UserName
        {
            get; set;
        }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Core.Resources.Label))]
        public string Password
        {
            get; set;
        }

        [Display(Name = "RememberMe", ResourceType = typeof(Core.Resources.Label))]
        public bool RememberMe
        {
            get; set;
        }
    }
}