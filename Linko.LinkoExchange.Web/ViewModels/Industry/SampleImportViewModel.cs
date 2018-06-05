using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Web.ViewModels.Shared;

namespace Linko.LinkoExchange.Web.ViewModels.Industry
{
    [Validator(validatorType:typeof(SampleImportViewModelValidator))]
    public class SampleImportViewModel
    {
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

        public SampleImportStep CurrentSampleImportStep { get; set; }

        [Display(Name = "Data Provider")]
        public int SelectedDataSourceId { get; set; }

        public string SelectedDataSourceName { get; set; }

        [Display(Name = "File Name")]
        public string SelectedFileName { get; set; }

        public int? ImportTempFileId { get; set; }

        public SampleImportDto SampleImportDto { get; set; } //TODO:Remove later

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

        [Display(Name = "Default Sample Type")]
        public int SelectedDefaultSampleTypeId { get; set; }

        public string SelectedDefaultSampleTypeName { get; set; }

        #endregion
    }

    public class StepSelectDataSourceViewModel
    {
        #region public properties

        public List<SelectListItem> AvailableDataSources { get; set; }
        public List<ListItemDto> AvailableDataProviders { get; set; }

        #endregion
    }

    public class StepSelectFileViewModel { }

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

    public class ErrorWithRowNumberViewModel
    {
        #region public properties

        public string ErrorMessage { get; set; }
        public string RowNumbers { get; set; }

        #endregion
    }

    public class SampleImportViewModelValidator : AbstractValidator<SampleImportViewModel>
    {
        #region static fields and constants

        public const string CannotFileImportFile = "Cannot find import file";

        #endregion

        #region constructors and destructor

        public SampleImportViewModelValidator()
        {
            RuleFor(x => x.SelectedDataSourceId).GreaterThan(valueToCompare:0).WithMessage(errorMessage:"{PropertyName} is required.");

            When(x => x.CurrentSampleImportStep > SampleImportViewModel.SampleImportStep.SelectFile,
                 () =>
                 {
                     RuleFor(x => x.ImportTempFileId).NotNull().GreaterThan(valueToCompare:0)
                                                     .WithMessage(errorMessage:CannotFileImportFile);
                 });
            When(x => x.CurrentSampleImportStep == SampleImportViewModel.SampleImportStep.SelectDataDefault
                      && !string.IsNullOrEmpty(value:x.SelectedDefaultMonitoringPointName),
                 () =>
                 {
                     RuleFor(x => x.SelectedDefaultMonitoringPointId)
                         .GreaterThan(valueToCompare:0).WithMessage(errorMessage:"{PropertyName} is required.");
                 });
            When(x => x.CurrentSampleImportStep == SampleImportViewModel.SampleImportStep.SelectDataDefault
                      && !string.IsNullOrEmpty(value:x.SelectedDefaultCollectionMethodName),
                 () =>
                 {
                     RuleFor(x => x.SelectedDefaultCollectionMethodId)
                         .GreaterThan(valueToCompare:0).WithMessage(errorMessage:"{PropertyName} is required.");
                 });
            When(x => x.CurrentSampleImportStep == SampleImportViewModel.SampleImportStep.SelectDataDefault
                      && !string.IsNullOrEmpty(value:x.SelectedDefaultSampleTypeName),
                 () =>
                 {
                     RuleFor(x => x.SelectedDefaultSampleTypeId)
                         .GreaterThan(valueToCompare:0).WithMessage(errorMessage:"{PropertyName} is required.");
                 });
        }

        #endregion
    }
}