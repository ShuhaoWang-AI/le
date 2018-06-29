using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Web.ViewModels.Industry; 

namespace Linko.LinkoExchange.Web.ViewModels.Shared
{
    [Validator(validatorType:typeof(SampleViewModelValidator))]
    public class SampleViewModel
    {
        #region public properties

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

	    [Display(Name = "Start Date")]
	    public string StartDateTimeLocalDisplayText => StartDateTimeLocal.ToString("MM/dd/yyyy hh:mm tt");

	    [Display(Name = "End Date")]
        public DateTime EndDateTimeLocal { get; set; }

		[Display(Name = "End Date")]
		public string EndDateTimeLocalDisplayText => EndDateTimeLocal.ToString("MM/dd/yyyy hh:mm tt");

	    [Display(Name = "Is Ready to Report")]
        public bool IsReadyToReport { get; set; }

        [Display(Name = "Status")]
        public SampleStatusName SampleStatusName { get; set; }

        [Display(Name = "Created/Modified Date")]
        public DateTime LastModificationDateTimeLocal { get; set; }

        [Display(Name = "Last Modified By")]
        public string LastModifierUserName { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Last Submitted")]
        public DateTime? LastSubmitted { get; set; }

        [Display(Name = "Flow")]
        public string FlowValue { get; set; }

        [Display(Name = "Flow Unit")]
        public int? FlowUnitId { get; set; }

        public string FlowUnitName { get; set; }
        public IEnumerable<UnitDto> FlowUnitValidValues { get; set; }

	    [Display(Name = "Flow")]
	    public string FlowValueAndFlow => string.IsNullOrWhiteSpace(FlowValue) ? "" : $"{FlowValue} {FlowUnitName}";

		public ImportStatus ImportStatus => Id.HasValue ? ImportStatus.Update : ImportStatus.New;

	    [Display(Name = "Import Status")]
	    public string Status => ImportStatus.ToString();


		public IList<SelectListItem> AvailableFlowUnits
        {
            get
            {
                var availableFlowUnits = FlowUnitValidValues.Select(c => new SelectListItem
                                                                         {
                                                                             Text = c.Name,
                                                                             Value = c.UnitId.ToString(),
                                                                             Selected = c.UnitId.Equals(obj:FlowUnitId)
                                                                         }).ToList();

                availableFlowUnits.Insert(index:0, item:new SelectListItem {Text = @"Select Flow Unit", Value = "0"});
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

        [Display(Name = " ")]
        public string SampleOverallCompliance { get; set; }
        public string SampleOverallComplianceComment { get; set; }

		public string Identifier { get; set; }
		public SampleComplianceSummaryViewModel SampleComplianceSummary { get; set; }

	    #endregion
	}


	public enum ImportStatus
	{
		New,
		Update,
		ExistingUnchanged
	}

    public class SampleViewModelValidator : AbstractValidator<SampleViewModel>
    {
        #region constructors and destructor

        public SampleViewModelValidator()
        {
            //CollectionMethodId
            RuleFor(x => x.CollectionMethodId).NotNull().WithMessage(errorMessage:"{PropertyName} is required.");

            //CtsEventTypeId
            RuleFor(x => x.CtsEventTypeId).NotNull().WithMessage(errorMessage:"{PropertyName} is required.");

            //StartDateTimeLocal
            RuleFor(x => x.StartDateTimeLocal).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");

            //EndDateTimeLocal
            RuleFor(x => x.EndDateTimeLocal).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");

            //FlowUnitId
            RuleFor(x => x.FlowUnitId).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.").GreaterThan(valueToCompare:0)
                                      .When(x => !string.IsNullOrWhiteSpace(value:x.FlowValue)).WithMessage(errorMessage:"{PropertyName} is required.");
        }

        #endregion
    }
}