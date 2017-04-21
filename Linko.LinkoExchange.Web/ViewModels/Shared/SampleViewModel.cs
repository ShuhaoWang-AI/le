﻿using System;
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

        [Display(Name = "Sample Start")]
        public DateTime StartDateTimeLocal { get; set; }

        [Display(Name = "Sample End")]
        public DateTime EndDateTimeLocal { get; set; }

        [Display(Name = "Is Ready to Report")]
        public bool IsReadyToReport { get; set; }

        [Display(Name = "Status")]
        public SampleStatusName SampleStatusName { get; set; }

        [Display(Name = "Created/Modified Date")]
        public DateTime LastModificationDateTimeLocal { get; set; }

        [Display(Name = "Last Modified By")]
        public string LastModifierUserName { get; set; }

        [Display(Name = "Sample Flow")]
        public string FlowValue { get; set; }

        public int? FlowUnitId { get; set; }
        public string FlowUnitName { get; set; }
        public IEnumerable<UnitDto> FlowUnitValidValues { get; set; }

        public IList<SelectListItem> AvailableFlowUnits => FlowUnitValidValues.Select(c => new SelectListItem
                                                                                           {
                                                                                               Text = c.Name,
                                                                                               Value = c.UnitId.ToString(),
                                                                                               Selected = c.UnitId.Equals(obj:FlowUnitId)
                                                                                           }).ToList();

        public string ResultQualifierValidValues { get; set; }

        public IList<SelectListItem> AvailableResultQualifierValidValues
        {
            get
            {
                var selectedValues = ResultQualifierValidValues?.Split(',').ToList() ?? new List<string> {""};
                var options = selectedValues.Select(
                                                    x => new SelectListItem
                                                         {
                                                             Text = x,
                                                             Value = x
                                                         }
                                                   ).ToList();
                return options;
            }
        }

        public double MassLoadingConversionFactorPounds { get; set; }
        public int MassLoadingCalculationDecimalPlaces { get; set; }
        public bool IsMassLoadingResultToUseLessThanSign { get; set; }
        public int MassLoadingMassLoadingUnitId { get; set; }
        public string MassLoadingMassLoadingUnitName { get; set; }

        // public IEnumerable<SampleResultViewModel> SampleResults { get; set; }
        public List<ParameterGroupViewModel> ParameterGroups { get; set; }
        public List<ParameterViewModel> AllParameters { get; set; }
    }

    public class SampleViewModelValidator:AbstractValidator<SampleViewModel> {}
}