using System;
using System.ComponentModel.DataAnnotations;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Web.ViewModels.Shared;

namespace Linko.LinkoExchange.Web.ViewModels.Industry
{

    public interface IDataSourceTranslationViewModel
    {
        int? Id { get; set; }
        bool IsTranslated { get; }
        int DataSourceId { get; set; }
        string DataSourceTerm { get; set; }
        DataSourceTranslationType TranslationType { get; set; }
        DropdownOptionViewModel TranslatedItem { get; set; }
    }

    public class DataSourceTranslationViewModelHelper
    {
        public static IDataSourceTranslationViewModel Create(DataSourceTranslationType translationType, DropdownOptionViewModel defaultValue = null)
        {
            switch (translationType)
            {
                case DataSourceTranslationType.MonitoringPoint:
                    return new DataSourceMonitoringPointTranslationViewModel
                           {
                               TranslatedItem = defaultValue,
                               MonitoringPoint = defaultValue
                           };
                case DataSourceTranslationType.SampleType:
                    return new DataSourceSampleTypeTranslationViewModel
                           {
                               TranslatedItem = defaultValue,
                               SampleType = defaultValue
                           };
                case DataSourceTranslationType.CollectionMethod:
                    return new DataSourceCollectionMethodTranslationViewModel
                           {
                               TranslatedItem = defaultValue,
                               CollectionMethod = defaultValue
                           };
                case DataSourceTranslationType.Parameter:
                    return new DataSourceParameterTranslationViewModel
                           {
                               TranslatedItem = defaultValue,
                               Parameter = defaultValue
                           };
                case DataSourceTranslationType.Unit:
                    return new DataSourceUnitTranslationViewModel
                           {
                               TranslatedItem = defaultValue,
                               Unit = defaultValue
                           };
                default: throw new NotImplementedException(message: $"DataSourceTranslationType {translationType} is unsupported");
            }
        }

        public static T To<T>(IDataSourceTranslationViewModel viewModel) where T : IDataSourceTranslationViewModel, new()
        {
            return new T
                   {
                       Id = viewModel.Id,
                       DataSourceId = viewModel.DataSourceId,
                       DataSourceTerm = viewModel.DataSourceTerm,
                       TranslationType = viewModel.TranslationType,
                       TranslatedItem = viewModel.TranslatedItem
                   };
        }
    }

    public abstract class DataSourceTranslationViewModel : IDataSourceTranslationViewModel
    {
        #region public properties

        [ScaffoldColumn(scaffold:false)]
        public int? Id { get; set; }

        public bool IsTranslated
        {
            get { return Id.HasValue; }
        }

        public int DataSourceId { get; set; }

        [Display(Name = "Import file term")]
        public string DataSourceTerm { get; set; }

        public DataSourceTranslationType TranslationType { get; set; }

        public DropdownOptionViewModel TranslatedItem { get; set; }

        #endregion
    }

    public class DataSourceMonitoringPointTranslationViewModel : DataSourceTranslationViewModel
    {
        #region public properties

        [UIHint(uiHint: "IndustryMonitoringPointEditor")]
        [Display(Name = "LinkoExchange term")]
        public DropdownOptionViewModel MonitoringPoint { get; set; }

        #endregion
    }

    public class DataSourceSampleTypeTranslationViewModel : DataSourceTranslationViewModel
    {
        #region public properties

        [UIHint(uiHint:"AuthoritySampleTypeEditor")]
        [Display(Name = "LinkoExchange term")]
        public DropdownOptionViewModel SampleType { get; set; }

        #endregion
    }

    public class DataSourceCollectionMethodTranslationViewModel : DataSourceTranslationViewModel
    {
        #region public properties

        [UIHint(uiHint:"AuthorityCollectionMethodEditor")]
        [Display(Name = "LinkoExchange term")]
        public DropdownOptionViewModel CollectionMethod { get; set; }

        #endregion
    }

    public class DataSourceParameterTranslationViewModel : DataSourceTranslationViewModel
    {
        #region public properties

        [UIHint(uiHint:"AuthorityParameterEditor")]
        [Display(Name = "LinkoExchange term")]
        public DropdownOptionViewModel Parameter { get; set; }

        #endregion
    }

    public class DataSourceUnitTranslationViewModel : DataSourceTranslationViewModel
    {
        #region public properties

        [UIHint(uiHint:"AuthorityUnitEditor")]
        [Display(Name = "LinkoExchange term")]
        public DropdownOptionViewModel Unit { get; set; }

        #endregion
    }
}