using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentValidation;
using FluentValidation.Attributes;
using Linko.LinkoExchange.Services.Base;

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

	    [Display(Name = "Result")]
	    public string DisplayValue => GetDisplayValue(qualifier:Qualifier, value:Value, unit:UnitName);

	    private static string GetDisplayValue(string qualifier, string value, string unit)
	    {
		    if (!string.IsNullOrWhiteSpace(value: qualifier) && new[] { "ND", "NF" }.ToList().CaseInsensitiveContains(value: qualifier))
		    {
			    return $"{qualifier}";
		    }

		    return string.IsNullOrWhiteSpace(value: value) ? "" : $"{qualifier} {value} {unit}";
        }

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

		[Display(Name = "Analysis Date & Time")]
		public string AnalysisDateTimeLocalDisplayText => AnalysisDateTimeLocal?.ToString("MM/dd/yyyy hh:mm tt") ?? "";

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

	    [Display(Name = "Mass Loading Result")]
	    public string DisplayMassLoadingValue => GetDisplayValue(qualifier:MassLoadingQualifier, value:MassLoadingValue, unit:MassLoadingUnitName);

		public string ConcentrationResultCompliance { get; set; }
        public string ConcentrationResultComplianceComment { get; set; }
        public string MassResultCompliance { get; set; }
        public string MassResultComplianceComment { get; set; }

	    public ImportStatus ImportStatus
	    {

		    get
		    {
			    if (Id.HasValue && ExistingUnchanged)
			    {
				    return ImportStatus.ExistingUnchanged;
			    }
				
				return Id.HasValue? ImportStatus.Update: ImportStatus.New;
			}  
	    } 

	    public string Status => ImportStatus.ToString();

		public bool ExistingUnchanged { get; set; }

		#endregion
	}

    public class SampleResultViewModelValidator : AbstractValidator<SampleResultViewModel> { }
}