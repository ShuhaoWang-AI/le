using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Web.ViewModels.Shared
{
    [Validator(validatorType:typeof(SampleViewModelValidator))]
    public class SampleViewModel
    {
        [ScaffoldColumn(scaffold:false)]
        public int? Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        public int MonitoringPointId { get; set; }

        [Display(Name = "Monitoring Point")]
        public string MonitoringPointName { get; set; }

        [Display(Name = "Sample Type")]
        public int CtsEventTypeId { get; set; }

        [Display(Name = "Sample Type")]
        public string CtsEventTypeName { get; set; }

        public IList<SelectListItem> AvailableCtsEventTypes { get; set; }

        [Display(Name = "Collection Method")]
        public int CollectionMethodId { get; set; }

        [Display(Name = "Collection Method")]
        public string CollectionMethodName { get; set; }

        public IList<SelectListItem> AvailableCollectionMethods { get; set; }

        [Display(Name = "Lab Sample ID")]
        public string LabSampleIdentifier { get; set; }

        [Display(Name = "Start Date")]
        public DateTime StartDateTimeLocal { get; set; }

        [Display(Name = "End Date")]
        public DateTime EndDateTimeLocal { get; set; }

        [Display(Name = "Is Ready to Report")]
        public bool IsReadyToReport { get; set; }

        [Display(Name = "Status")]
        public SampleStatusName SampleStatusName { get; set; }

        [Display(Name = "Created/Modified Date")]
        public DateTime LastModificationDateTimeLocal { get; set; }

        [Display(Name = "Last Modified By")]
        public string LastModifierUserName { get; set; }

        [Editable(allowEdit: false)]
        [Display(Name = "Last Submitted")]
        public DateTime? LastSubmitted { get; set; }

        [Display(Name = "Flow")]
        public string FlowValue { get; set; }

        public int? FlowUnitId { get; set; }
        public string FlowUnitName { get; set; }
        public IEnumerable<UnitDto> FlowUnitValidValues { get; set; }

        public IList<SelectListItem> AvailableFlowUnits
        {
            get
            {
                var availableFlowUnits = FlowUnitValidValues.Select(c => new SelectListItem
                                                                         {
                                                                             Text = c.Name,
                                                                             Value = c.UnitId.ToString(),
                                                                             Selected = c.UnitId.Equals(obj: FlowUnitId)
                                                                         }).ToList();

                availableFlowUnits.Insert(index: 0, item: new SelectListItem {Text = @"Select Flow Unit", Value = "0"});
                return availableFlowUnits;
            }
        }

        public string ResultQualifierValidValues { get; set; }

        public IList<SelectListItem> AvailableResultQualifierValidValues
        {
            get
            {
                var selectedValues = ResultQualifierValidValues?.Split(',').ToList() ?? new List<string> {""};

                var options = new List<SelectListItem> {new SelectListItem {Text = " ", Value = "NUMERIC"}};

                options.AddRange(collection:selectedValues.Select(
                                                       x => new SelectListItem
                                                            {
                                                                Text = x,
                                                                Value = x
                                                            }
                                                      ).ToList());
                return options;
            }
        }

        public double MassLoadingConversionFactorPounds { get; set; }
        public int MassLoadingCalculationDecimalPlaces { get; set; }
        public bool IsMassLoadingResultToUseLessThanSign { get; set; }
        public int MassLoadingMassLoadingUnitId { get; set; }
        public string MassLoadingMassLoadingUnitName { get; set; }

        public IEnumerable<SampleResultViewModel> SampleResults { get; set; }
        public IList<ParameterGroupViewModel> ParameterGroups { get; set; }
        public IList<ParameterViewModel> AllParameters { get; set; }
        public string CtsEventCategoryName { get; set; }

        [Display(Name = " ")]
        public bool IsAssociatedWithReportPackage { get; internal set; } // only to be used when displaying report package to show which samples are included
    }

    public class SampleViewModelValidator : AbstractValidator<SampleViewModel>
    {
        public SampleViewModelValidator()
        {
            //StartDateTimeLocal
            RuleFor(x => x.StartDateTimeLocal).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");
            //EndDateTimeLocal
            RuleFor(x => x.EndDateTimeLocal).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.").GreaterThan(x => x.StartDateTimeLocal)
                                            .WithMessage(errorMessage: "End date must after Start date");
            //ReportElementTypeId
            RuleFor(x => x.FlowUnitId).GreaterThan(valueToCompare:0).When(x => !x.FlowValue.Trim().Equals(value: "")).WithMessage(errorMessage:"{PropertyName} is required.");
        }
    }
}