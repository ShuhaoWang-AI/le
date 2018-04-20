using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;

namespace Linko.LinkoExchange.Web.ViewModels.Authority
{
    [Validator(validatorType:typeof(AuthorityUnitViewModelValidator))]
    public class AuthorityUnitViewModel
    {
        #region public properties

        [ScaffoldColumn(scaffold:false)]
        public int? Id { get; set; }

        [Display(Name = "Authority Unit")]
        public string Name { get; set; }
        
        [UIHint(uiHint:"SystemUnitEditor")]
        [Display(Name = "System Unit")]
        public SystemUnitViewModel SystemUnit { get; set; }

        [Display(Name = "Is Available to Industry")]
        public bool IsAvailableToRegulatee { get; set; }

        #endregion
    }

    public class AuthorityUnitViewModelValidator : AbstractValidator<AuthorityUnitViewModel> { }
}