using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Web.ViewModels.Industry
{
    [Validator(validatorType:typeof(NewSampleStep1ViewModelValidator))]
    public class NewSampleStep1ViewModel
    {
        #region public properties

        [Display(Name = "Monitoring Point")]
        public int SelectedMonitoringPointId { get; set; }

        public List<MonitoringPointViewModel> AllMonitoringPoints { get; set; }

        [Display(Name = "Start Date")]
        public DateTime? StartDateTimeLocal { get; set; }

        [Display(Name = "End Date")]
        public DateTime? EndDateTimeLocal { get; set; }

        //
        // Copied properties used when cloning a sample only
        //
        public bool IsClonedSample { get; set; }
        public IEnumerable<UnitDto> FlowUnitValidValues { get; set; }
        public IEnumerable<SampleResultViewModel> SampleResults { get; set; }
        public int CollectionMethodId { get; set; }
        public string LabSampleIdentifier { get; set; }
        public int CtsEventTypeId { get; set; }
        public string FlowValue { get; set; }
        public int? FlowUnitId { get; set; }
        #endregion
    }

    public class NewSampleStep1ViewModelValidator : AbstractValidator<NewSampleStep1ViewModel>
    {
        #region constructors and destructor

        public NewSampleStep1ViewModelValidator()
        {
            //SelectedMonitoringPointId
            RuleFor(x => x.SelectedMonitoringPointId).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");

            //StartDateTimeLocal
            RuleFor(x => x.StartDateTimeLocal).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");

            //EndDateTimeLocal
            RuleFor(x => x.EndDateTimeLocal).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.").GreaterThanOrEqualTo(x => x.StartDateTimeLocal)
                                            .WithMessage(errorMessage:"End date must be after Start date");
        }

        #endregion
    }
}