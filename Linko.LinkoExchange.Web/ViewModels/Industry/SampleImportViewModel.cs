using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using Telerik.Windows.Documents.Spreadsheet.Model;

namespace Linko.LinkoExchange.Web.ViewModels.Industry
{
    [Validator(validatorType:typeof(SampleImportViewModelValidator))]
    public class SampleImportViewModel
    {
        public enum SampleImportStep
        {
            SelectDataSource,
            SelectFile,
            FileValidation,
            SelectDataDefault,
            DataTranslationMonitoringPoint,
            DataTranslationCollectionMethod,
            DataTranslationSampleType,
            DataTranslationParameter,
            DataTranslationUnit,
            DataValidation,
            ShowPreImportOutput,
            ShowImportOutput
        }

        public SampleImportStep CurrentSampleImportStep { get; set; }

        [Display(Name = "Data Source")]
        public int SelectedDataSourceId{ get; set; }
        public string SelectedDataSourceName { get; set; }

        [Display(Name = "File Name")]
        public string SelectedFileName { get; set; }
        public int? ImportTempFileId { get; set; }
        public Workbook ImportFileWorkbook { get; set; }

        public StepSelectDataSourceViewModel StepSelectDataSource { get; set; }
        public StepSelectFileViewModel StepSelectFile { get; set; }
        public StepFileValidationViewModel StepFileValidation { get; set; }
    }


    public class StepSelectDataSourceViewModel
    {
        public List<SelectListItem> AvailableDataSources { get; set; }
    }
    public class StepSelectFileViewModel
    {
    }
    public class StepFileValidationViewModel
    {
        public IEnumerable<ErrorWithRowNumberViewModel> Errors { get; set; }
    }
    
    public class ErrorWithRowNumberViewModel
    {
        public string ErrorMessage { get; set; }
        public string RowNumbers { get; set; }

    }
    public class SampleImportViewModelValidator : AbstractValidator<SampleImportViewModel>
    {
        public SampleImportViewModelValidator()
        {
            RuleFor(x => x.SelectedDataSourceId).GreaterThan(valueToCompare:0).WithMessage(errorMessage:"{PropertyName} is required.");
        }
    }
}