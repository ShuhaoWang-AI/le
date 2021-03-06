﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Base;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using Newtonsoft.Json;

namespace Linko.LinkoExchange.Web.ViewModels.Industry
{
	[Validator(validatorType:typeof(SampleImportViewModelValidator))]
	public class SampleImportViewModel
	{
		#region constructors and destructor

		public SampleImportViewModel()
		{
			ImportId = Guid.NewGuid();
			CurrentSampleImportStep = SampleImportStep.SelectDataSource;
			SampleImportDto = new SampleImportDto
			                  {
				                  ImportId = ImportId
			                  };
		}

		#endregion

		#region public enums

		public enum SampleImportStep
		{
			SelectDataSource,
			SelectFile,
			FileValidation,
			SelectDataDefault,
			DataTranslations,
			DataValidation,
			ShowPreImportOutput,
			ShowImportOutput
		}

		#endregion

		#region public properties

		public Guid ImportId { get; set; }

		public SampleImportStep CurrentSampleImportStep { get; set; }

		[Display(Name = "Data Provider")]
		public int SelectedDataSourceId { get; set; }

		public string SelectedDataSourceName { get; set; }

		public int ImportTempFileId { get; set; }
		public int ImportFileId { get; set; }

		[Display(Name = "File Name")]
		public string SelectedFileName { get; set; }

		public List<SampleViewModel> Samples { get; set; }

		public StepSelectDataSourceViewModel StepSelectDataSource { get; set; }
		public StepSelectFileViewModel StepSelectFile { get; set; }
		public StepFileValidationViewModel StepFileValidation { get; set; }
		public StepDataValidationViewModel StepDataValidation { get; set; }

		[Display(Name = "Default Monitoring Point")]
		public int SelectedDefaultMonitoringPointId { get; set; }
        public string SelectedDefaultMonitoringPointName { get; set; }

		[Display(Name = "Default Collection Method")]
		public int SelectedDefaultCollectionMethodId { get; set; }

		public string SelectedDefaultCollectionMethodName { get; set; }

		public string FlowParameterName { get; set; }

		[Display(Name = "Default Sample Type")]
		public int SelectedDefaultSampleTypeId { get; set; }

		public string SelectedDefaultSampleTypeName { get; set; }

        public List<ImportDataTranslationViewModel> DataTranslations { get; set; }

		public SampleImportDto SampleImportDto { get; set; }

		public ImportSummaryViewModel ImportSummary { get; set; }

		#endregion
	}

	public class StepSelectDataSourceViewModel
	{
		#region public properties

		public List<SelectListItem> AvailableDataSources { get; set; }
		public List<ListItemDto> AvailableDataProviders { get; set; }

		#endregion
	}

	public class StepSelectFileViewModel {}

	public class StepFileValidationViewModel
	{
		#region public properties

		public List<ErrorWithRowNumberViewModel> Errors { get; set; }

		#endregion
	}

	public class StepDataValidationViewModel
	{
		#region public properties

		public List<ErrorWithRowNumberViewModel> Errors { get; set; }

		#endregion
	}

    public class ImportDataTranslationViewModel
    {
        #region public properties

        public List<IDataSourceTranslationViewModel> DataTranslations { get; set; }
        public string Title { get; set; }
        public int NumberOfMissingTranslations { get; set; }
        public int NumberOfExistingTranslations { get; set; }
        public int NumberOfTotalTranslations => NumberOfMissingTranslations + NumberOfExistingTranslations;
        public DataSourceTranslationType TranslationType { get; set; }

		#endregion
	}

	public class ErrorWithRowNumberViewModel
	{
		#region public properties

		public string ErrorMessage { get; set; }
		public string RowNumbers { get; set; }

		#endregion
	}

	public class SampleImportViewModelValidator : AbstractValidator<SampleImportViewModel>
	{
		#region constructors and destructor

		public SampleImportViewModelValidator()
		{
			RuleFor(x => x.SelectedDataSourceId).GreaterThan(valueToCompare:0).WithMessage(errorMessage:ErrorConstants.Validator.PropertyNameIsRequired);

			When(x => x.CurrentSampleImportStep > SampleImportViewModel.SampleImportStep.SelectFile,
			     () =>
			     {
				     RuleFor(x => x.ImportTempFileId).NotNull().GreaterThan(valueToCompare:0)
				                                     .WithMessage(errorMessage:ErrorConstants.SampleImport.CannotFindImportFile);
			     });
			When(x => x.CurrentSampleImportStep == SampleImportViewModel.SampleImportStep.SelectDataDefault
			          && !string.IsNullOrEmpty(value:x.SelectedDefaultMonitoringPointName),
			     () =>
			     {
				     RuleFor(x => x.SelectedDefaultMonitoringPointId)
					     .GreaterThan(valueToCompare:0).WithMessage(errorMessage:ErrorConstants.Validator.PropertyNameIsRequired);
			     });
			When(x => x.CurrentSampleImportStep == SampleImportViewModel.SampleImportStep.SelectDataDefault
			          && !string.IsNullOrEmpty(value:x.SelectedDefaultCollectionMethodName),
			     () =>
			     {
				     RuleFor(x => x.SelectedDefaultCollectionMethodId)
					     .GreaterThan(valueToCompare:0).WithMessage(errorMessage:ErrorConstants.Validator.PropertyNameIsRequired);
			     });
			When(x => x.CurrentSampleImportStep == SampleImportViewModel.SampleImportStep.SelectDataDefault
			          && !string.IsNullOrEmpty(value:x.SelectedDefaultSampleTypeName),
			     () =>
			     {
				     RuleFor(x => x.SelectedDefaultSampleTypeId)
					     .GreaterThan(valueToCompare:0).WithMessage(errorMessage:ErrorConstants.Validator.PropertyNameIsRequired);
			     });
		}

		#endregion
	}



	public class ImportSummaryViewModel
	{
		public ImportSummaryViewModel()
		{
			SampleComplianceSummary = new SampleComplianceSummaryViewModel();
		}

		#region public properties

		public int NewDraftSampleCount { get; set; }
        public int UpdateDraftSampleCont { get; set; }
		[JsonIgnore]
		public string UpdateDraftSampleCountDescription => UpdateDraftSampleCont > 1 ? "updates" : "update";
		[JsonIgnore]
        public string Updated => "updated";
        [JsonIgnore]
		public int SampleCount => NewDraftSampleCount + UpdateDraftSampleCont;
		[JsonIgnore]
		public string SampleCountImportDescription => SampleCount > 1 ? "Draft Samples are imported" : "Draft Sample is imported";
		[JsonIgnore]
		public string SampleCountPreviewDescription => SampleCount > 1 ? "Draft Samples are ready to import" : "Draft Sample is ready to import";
        public int NewSampleResultCount { get; set; }
        public int UpdateSampleResultCount { get; set; }
		public string UpdateSampleResultCountDescription => UpdateSampleResultCount > 1 ? "updates" : "update"; 
        [JsonIgnore]
        public int SampleResultCount => NewSampleResultCount + UpdateSampleResultCount;
		[JsonIgnore]
		public string SampleResultCountImportDescription => SampleResultCount > 1 ? "Sample Results are imported" : "Sample Result is imported";
        [JsonIgnore]
		public string SampleResultCountPreviewDescription => SampleResultCount > 1 ? "Sample Results are ready to import" : "Sample Result is ready to import";
        public SampleComplianceSummaryViewModel SampleComplianceSummary { get; set; } 
		#endregion
	}


	public class SampleComplianceSummaryViewModel
	{
		public int GoodConcentrationComplianceCount { get; set; }
		public int BadConcentrationComplianceCount { get; set; }
		public int UnknownConcentrationComplianceCount { get; set; }
		public int GoodMassLoadingComplianceCount { get; set; }
		public int BadMassLoadingComplianceCount { get; set; }
		public int UnknownMassLoadingComplianceCount { get; set; }

		[JsonIgnore]
        public int TotalBadComplianceCount => BadConcentrationComplianceCount + BadMassLoadingComplianceCount;
		[JsonIgnore]
		public string TotalBadComplianceDescription => string.Format(format: "{0} ready to import {1} not in compliance", 
		                                                             arg0:TotalBadComplianceCount,  
																	 arg1:TotalBadComplianceCount > 1 ? " results are " : " result is");

		[JsonIgnore]
		public string AllResultsAreInCompliance => "All ready to import results are in compliance";


        [JsonIgnore]
		private int TotalGoodComplianceCount => GoodConcentrationComplianceCount + 
		                                       UnknownConcentrationComplianceCount + 
		                                       GoodMassLoadingComplianceCount + 
		                                       UnknownMassLoadingComplianceCount;

		[JsonIgnore]
		private int TotalComplianceCount => TotalGoodComplianceCount + TotalBadComplianceCount;
		[JsonIgnore]
		public int TotalGoodPercentage => TotalComplianceCount > 0 ?
			                                  Convert.ToInt32(value: Convert.ToDecimal(value: TotalGoodComplianceCount) / Convert.ToDecimal(value: TotalComplianceCount) * 100) : 100;
		[JsonIgnore]
		public int TotalBadPercentage => 100 - TotalGoodPercentage;
        [JsonIgnore]
        public string FinalComplianceDescription => string.Format(format: "{0} of {1} imported Sample {2} in compliance", 
                                                                  arg0:TotalGoodComplianceCount, 
                                                                  arg1:TotalComplianceCount, 
                                                                  arg2: TotalGoodComplianceCount > 1 ? "Results are" : "Result is") ;

    }
}