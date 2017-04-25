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

        [Display(Name = "Sample Start")]
        public DateTime StartDateTimeLocal { get; set; }

        [Display(Name = "Sample End")]
        public DateTime EndDateTimeLocal { get; set; }
    }

    public class NewSampleStep1ViewModelValidator:AbstractValidator<NewSampleStep1ViewModel> {}
}