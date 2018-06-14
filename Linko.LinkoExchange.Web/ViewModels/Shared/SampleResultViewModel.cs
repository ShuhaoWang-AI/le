using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;

namespace Linko.LinkoExchange.Web.ViewModels.Shared
{
    [Validator(validatorType:typeof(SampleResultViewModelValidator))]
    public class SampleResultViewModel
    {
        #region public properties

        [ScaffoldColumn(scaffold:false)]
        [Display(Name = "Sample Result Id")]
        public int? Id { get; set; }

        [Display(Name = "Parameter Id")]
        public int ParameterId { get; set; }

        [Display(Name = "Parameter Name")]
        public string ParameterName { get; set; }

        [Display(Name = "Qualifier")]
        public string Qualifier { get; set; }

        [Display(Name = "Result")]
        public string Value { get; set; }

        [Display(Name = "Unit")]
        public int UnitId { get; set; }

        [Display(Name = "Unit")]
        public string UnitName { get; set; }

        [Display(Name = "MDL")]
        public string EnteredMethodDetectionLimit { get; set; }

        [Display(Name = "Analysis Method")]
        public string AnalysisMethod { get; set; }

        [Display(Name = "Analysis Date & Time")]
        public DateTime? AnalysisDateTimeLocal { get; set; }

        [Display(Name = "Calculate Loading")]
        public bool IsCalcMassLoading { get; set; }

        [ScaffoldColumn(scaffold:false)]
        [Display(Name = "Mass Loading Sample Result Id")]
        public int? MassLoadingSampleResultId { get; set; }

        public string MassLoadingQualifier { get; set; }

        [Display(Name = "Mass Loading Result")]
        public string MassLoadingValue { get; set; }

        public int MassLoadingUnitId { get; set; }
        public string MassLoadingUnitName { get; set; }

        public string ConcentrationResultCompliance { get; set; }
        public string ConcentrationResultComplianceComment { get; set; }
        public string MassResultCompliance { get; set; }
        public string MassResultComplianceComment { get; set; }
		
		public ImportStatus ImportStatus => Id.HasValue ? ImportStatus.Updated : ImportStatus.New;
	    public string Status => ImportStatus.ToString();

	    #endregion
    }

    public class SampleResultViewModelValidator : AbstractValidator<SampleResultViewModel> { }
}