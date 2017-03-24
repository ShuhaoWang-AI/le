using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Web.ViewModels.Shared
{
    [Validator(validatorType:typeof(ReportElementTypeViewModelValidator))]
    public class ReportElementTypeViewModel
    {
        [ScaffoldColumn(scaffold:false)]
        public int? Id { get; set; }

        [Display(Name = "Element Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        [DataType(dataType:DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "Category")]
        public ReportElementCategoryName CategoryName { get; set; }

        [Display(Name = "Certification Text")]
        public string Content { get; set; }

        public bool IsContentProvided { get; set; }

        public int CtsEventTypeId { get; set; }

        [Display(Name = "CTS Event Type")]
        public int CtsEventTypeName { get; set; }

        [Display(Name = "Created/Modified Date")]
        public DateTime LastModificationDateTimeLocal { get; set; }

        [Display(Name = "Last Modified By")]
        public string LastModifierUserName { get; set; }

        public class ReportElementTypeViewModelValidator:AbstractValidator<ReportElementTypeViewModel>
        {
            public ReportElementTypeViewModelValidator()
            {
                //Name
                RuleFor(x => x.Name).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");
            }
        }
    }
}