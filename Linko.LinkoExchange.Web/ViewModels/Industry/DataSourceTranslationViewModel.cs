using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Web.ViewModels.Shared;

namespace Linko.LinkoExchange.Web.ViewModels.Industry
{
    public class DataSourceTranslationViewModel
    {
        #region public properties

        [ScaffoldColumn(scaffold: false)]
        public int? Id { get; set; }

        public int DataSourceId { get; set; }

        [Display(Name = "This was in the file")]
        public string DataSourceTerm { get; set; }

        public DataSourceTranslationType TranslationType { get; set; }

        public virtual DropdownOptionViewModel TranslatedItem { get; set; }

        #endregion

        public static DataSourceTranslationViewModel Create(DataSourceTranslationType translationType)
        {
            switch (translationType)
            {
                case DataSourceTranslationType.MonitoringPoint: return new DataSourceMonitoringPointTranslationViewModel();
                case DataSourceTranslationType.SampleType: return new DataSourceSampleTypeTranslationViewModel();
                case DataSourceTranslationType.CollectionMethod: return new DataSourceCollectionMethodTranslationViewModel();
                case DataSourceTranslationType.Parameter: return new DataSourceParameterTranslationViewModel();
                case DataSourceTranslationType.Unit: return new DataSourceUnitTranslationViewModel();
                default: throw new NotImplementedException(message: $"DataSourceTranslationType {translationType} is unsupported");
            }
        }
    }

    public abstract class DataSourceTranslationValidator<T> : AbstractValidator<T> where T : DataSourceTranslationViewModel
    {
        #region constructors and destructor

        protected DataSourceTranslationValidator()
        {
            RuleFor(x => x.DataSourceTerm).NotEmpty().WithMessage(errorMessage: "Should not be empty");
        }

        #endregion
    }

    [Validator(validatorType: typeof(DataSourceMonitoringPointValidator))]
    public class DataSourceMonitoringPointTranslationViewModel : DataSourceTranslationViewModel
    {
        #region public properties

        [UIHint(uiHint: "AuthorityMonitoringPointEditor")]
        [Display(Name = "Choose which Monitoring Point this is")]
        public DropdownOptionViewModel MonitoringPoint { get; set; }

        public override DropdownOptionViewModel TranslatedItem
        {
            get { return this.MonitoringPoint; }
            set { MonitoringPoint = value; }
        }

        #endregion
    }

    public class DataSourceMonitoringPointValidator : DataSourceTranslationValidator<DataSourceMonitoringPointTranslationViewModel>
    {
        #region constructors and destructor

        public DataSourceMonitoringPointValidator() : base()
        {
            RuleFor(x => x.TranslatedItem).NotNull().WithMessage(errorMessage: "The Monitoring Point is required.");
            RuleFor(x => x.TranslatedItem.Id).NotNull().WithMessage(errorMessage: "The Monitoring Point is required.");
        }

        #endregion
    }

    [Validator(validatorType: typeof(DataSourceSampleTypeValidator))]
    public class DataSourceSampleTypeTranslationViewModel : DataSourceTranslationViewModel
    {
        #region public properties

        [UIHint(uiHint: "AuthoritySampleTypeEditor")]
        [Display(Name = "Choose which Sample Type this is")]
        public DropdownOptionViewModel SampleType { get; set; }

        public override DropdownOptionViewModel TranslatedItem
        {
            get { return this.SampleType; }
            set { SampleType = value; }
        }

        #endregion
    }

    public class DataSourceSampleTypeValidator : DataSourceTranslationValidator<DataSourceSampleTypeTranslationViewModel>
    {
        #region constructors and destructor

        public DataSourceSampleTypeValidator() : base()
        {
            RuleFor(x => x.TranslatedItem).NotNull().WithMessage(errorMessage: "The Sample Type is required.");
            RuleFor(x => x.TranslatedItem.Id).NotNull().WithMessage(errorMessage: "The Sample Type is required.");
        }

        #endregion
    }

    [Validator(validatorType: typeof(DataSourceCollectionMethodValidator))]
    public class DataSourceCollectionMethodTranslationViewModel : DataSourceTranslationViewModel
    {
        #region public properties

        [UIHint(uiHint: "AuthorityCollectionMethodEditor")]
        [Display(Name = "Choose which Collection Method this is")]
        public DropdownOptionViewModel CollectionMethod { get; set; }

        public override DropdownOptionViewModel TranslatedItem
        {
            get { return this.CollectionMethod; }
            set { CollectionMethod = value; }
        }
        #endregion
    }

    public class DataSourceCollectionMethodValidator : DataSourceTranslationValidator<DataSourceCollectionMethodTranslationViewModel>
    {
        #region constructors and destructor

        public DataSourceCollectionMethodValidator() : base()
        {
            RuleFor(x => x.TranslatedItem).NotNull().WithMessage(errorMessage: "The Collection Method is required.");
            RuleFor(x => x.TranslatedItem.Id).NotNull().WithMessage(errorMessage: "The Collection Method is required.");
        }

        #endregion
    }

    [Validator(validatorType: typeof(DataSourceParameterValidator))]
    public class DataSourceParameterTranslationViewModel : DataSourceTranslationViewModel
    {
        #region public properties

        [UIHint(uiHint: "AuthorityParameterEditor")]
        [Display(Name = "Choose which Parameter this is")]
        public DropdownOptionViewModel Parameter { get; set; }

        public override DropdownOptionViewModel TranslatedItem
        {
            get { return this.Parameter; }
            set { Parameter = value; }
        }
        #endregion
    }

    public class DataSourceParameterValidator : DataSourceTranslationValidator<DataSourceParameterTranslationViewModel>
    {
        #region constructors and destructor

        public DataSourceParameterValidator() : base()
        {
            RuleFor(x => x.TranslatedItem).NotNull().WithMessage(errorMessage: "The Parameter is required.");
            RuleFor(x => x.TranslatedItem.Id).NotNull().WithMessage(errorMessage: "The Parameter is required.");
        }

        #endregion
    }

    [Validator(validatorType: typeof(DataSourceUnitValidator))]
    public class DataSourceUnitTranslationViewModel : DataSourceTranslationViewModel
    { 
        #region public properties

        [UIHint(uiHint: "AuthorityUnitEditor")]
        [Display(Name = "Choose which Unit this is")]
        public DropdownOptionViewModel Unit { get; set; }

        public override DropdownOptionViewModel TranslatedItem
        {
            get { return this.Unit; }
            set { Unit = value; }
        }
        #endregion
    }

    public class DataSourceUnitValidator : DataSourceTranslationValidator<DataSourceUnitTranslationViewModel>
    {
        #region constructors and destructor

        public DataSourceUnitValidator() : base()
        {
            RuleFor(x => x.TranslatedItem).NotNull().WithMessage(errorMessage: "The Unit is required.");
            RuleFor(x => x.TranslatedItem.Id).NotNull().WithMessage(errorMessage: "The Unit is required.");
        }

        #endregion
    }
}
