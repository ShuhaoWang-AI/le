using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;
using Linko.LinkoExchange.Web.ViewModels.Shared;

namespace Linko.LinkoExchange.Web.ViewModels.ReportPackage
{
    [Validator(validatorType:typeof(NewReportPackageViewModelValidator))]
    public class NewReportPackageViewModel
    {
        #region public properties

        [Display(Name = "Report Package Template")]
        public int SelectedReportPackageTemplateId { get; set; }

        public List<ReportPackageTemplateViewModel> AllReportPackageTemplates { get; set; }

        [Display(Name = "Start Date")]
        public DateTime? StartDateTimeLocal { get; set; }

        [Display(Name = "End Date")]
        public DateTime? EndDateTimeLocal { get; set; }

        #endregion
    }

    public class NewReportPackageViewModelValidator : AbstractValidator<NewReportPackageViewModel>
    {
        #region constructors and destructor

        public NewReportPackageViewModelValidator()
        {
            //SelectedMonitoringPointId
            RuleFor(x => x.SelectedReportPackageTemplateId).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");

            //StartDateTimeLocal
            RuleFor(x => x.StartDateTimeLocal).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");

            //EndDateTimeLocal
            RuleFor(x => x.EndDateTimeLocal).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.").GreaterThanOrEqualTo(x => x.StartDateTimeLocal)
                                            .WithMessage(errorMessage:"End date must be after Start date");
        }

        #endregion
    }
}