using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using Linko.LinkoExchange.Core.Enum;

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

        [Display(Name = "Sample Start")]
        public DateTime StartDateTimeLocal { get; set; }

        [Display(Name = "Sample End")]
        public DateTime EndDateTimeLocal { get; set; }

        //public bool IsCalculated { get; set; }

        [Display(Name = "Ready ")]
        public bool IsReadyToReport { get; set; }

        [Display(Name = "Status")]
        public SampleStatusName SampleStatusName { get; set; }

        [Display(Name = "Created/Modified Date")]
        public DateTime LastModificationDateTimeLocal { get; set; }

        [Display(Name = "Last Modified By")]
        public string LastModifierUserName { get; set; }

        [Display(Name = "Sample Flow")]
        public double? FlowValue { get; set; }
        public int? FlowValueDecimalPlaces { get; set; }

        [Display(Name = "Sample Flow")]
        public string FlowValueWithDecimalPlaces { get; set; }
        //get
            //{
            //    return FlowValue.HasValue && FlowValueDecimalPlaces.HasValue
            //               ? string.Format(string.Format("{{0:n{0}}}", FlowValueDecimalPlaces.Value), FlowValue.Value)
            //               : "";
            //}

        public int? FlowUnitId { get; set; }
        public string FlowUnitName { get; set; }
        public IList<SelectListItem> AvailableFlowUnits { get; set; }

       // public IEnumerable<SampleResultViewModel> SampleResults { get; set; }

        public double CurrentMassLoadingConversionFactorPounds { get; set; }
        public int CurrentMassLoadingCalculationDecimalPlaces { get; set; }
        public bool CurrentMassLoadingResultToUseLessThanSign { get; set; }
        public int CurrentMassLoadingMassLoadingUnitId { get; set; }
        public string CurrentMassLoadingMassLoadingUnitName { get; set; }
        
        public List<ParameterGroupViewModel> ParameterGroups { get; set; }
        public List<ParameterViewModel> AllParameters { get; set; }
    }

    public class SampleViewModelValidator:AbstractValidator<SampleViewModel>
    {
        public SampleViewModelValidator()
        {

        }
    }
}