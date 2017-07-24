using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;
using Linko.LinkoExchange.Web.ViewModels.Shared;

namespace Linko.LinkoExchange.Web.ViewModels.Industry
{
    [Validator(validatorType:typeof(NewSampleStep1ViewModelValidator))]
    public class NewSampleStep1ViewModel
    {
        [Display(Name = "Monitoring Point")]
        public int SelectedMonitoringPointId { get; set; }
        public List<MonitoringPointViewModel> AllMonitoringPoints { get; set; }

        [Display(Name = "Start Date")]
        public DateTime? StartDateTimeLocal { get; set; }

        [Display(Name = "End Date")]
        public DateTime? EndDateTimeLocal { get; set; }
    }

    public class NewSampleStep1ViewModelValidator:AbstractValidator<NewSampleStep1ViewModel> {
        public NewSampleStep1ViewModelValidator()
        {
            //SelectedMonitoringPointId
            RuleFor(x => x.SelectedMonitoringPointId).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");
            //StartDateTimeLocal
            RuleFor(x => x.StartDateTimeLocal).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");
            //EndDateTimeLocal
            RuleFor(x => x.EndDateTimeLocal).NotEmpty().WithMessage(errorMessage: "{PropertyName} is required.").GreaterThanOrEqualTo(x => x.StartDateTimeLocal)
                                            .WithMessage(errorMessage: "End date must be after Start date");
        }
    }
}