using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;

namespace Linko.LinkoExchange.Web.ViewModels.Authority
{
    [Validator(validatorType:typeof(SystemUnitViewModelValidator))]
    public class SystemUnitViewModel
    {
        #region public properties

        [ScaffoldColumn(scaffold:false)]
        public int? Id { get; set; }

        [Display(Name = "System Unit")]
        public string Name { get; set; }
        
        public string Description { get; set; }

        public int? UnitDimensionId { get; set; }

        public string UnitDimensionName { get; set; }

        #endregion
    }

    public class SystemUnitViewModelValidator : AbstractValidator<SystemUnitViewModel> { }
}