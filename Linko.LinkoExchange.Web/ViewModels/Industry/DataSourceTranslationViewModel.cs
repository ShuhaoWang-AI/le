using System;
using System.ComponentModel.DataAnnotations;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Web.ViewModels.Shared;

namespace Linko.LinkoExchange.Web.ViewModels.Industry
{
    public class DataSourceTranslationViewModel
    {
        #region public properties

        [ScaffoldColumn(scaffold:false)]
        public int? Id { get; set; }

        public int DataSourceId { get; set; }

        [Display(Name = "Import file term")]
        public string DataSourceTerm { get; set; }

        public DataSourceTranslationType TranslationType { get; set; }

        [UIHint(uiHint:"LinkoExchangeTermEditor")]
        [Display(Name = "LinkoExchange term")]
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
                default: throw new NotImplementedException(message:$"DataSourceTranslationType {translationType} is unsupported");
            }
        }
    }

    public class DataSourceMonitoringPointTranslationViewModel : DataSourceTranslationViewModel
    {
        #region public properties

        [UIHint(uiHint:"AuthorityMonitoringPointEditor")]
        [Display(Name = "LinkoExchange term")]
        public DropdownOptionViewModel MonitoringPoint { get; set; }

        public override DropdownOptionViewModel TranslatedItem
        {
            get { return MonitoringPoint; }
            set { MonitoringPoint = value; }
        }

        #endregion
    }

    public class DataSourceSampleTypeTranslationViewModel : DataSourceTranslationViewModel
    {
        #region public properties

        [UIHint(uiHint:"AuthoritySampleTypeEditor")]
        [Display(Name = "LinkoExchange term")]
        public DropdownOptionViewModel SampleType { get; set; }

        public override DropdownOptionViewModel TranslatedItem
        {
            get { return SampleType; }
            set { SampleType = value; }
        }

        #endregion
    }

    public class DataSourceCollectionMethodTranslationViewModel : DataSourceTranslationViewModel
    {
        #region public properties

        [UIHint(uiHint:"AuthorityCollectionMethodEditor")]
        [Display(Name = "LinkoExchange term")]
        public DropdownOptionViewModel CollectionMethod { get; set; }

        public override DropdownOptionViewModel TranslatedItem
        {
            get { return CollectionMethod; }
            set { CollectionMethod = value; }
        }

        #endregion
    }

    public class DataSourceParameterTranslationViewModel : DataSourceTranslationViewModel
    {
        #region public properties

        [UIHint(uiHint:"AuthorityParameterEditor")]
        [Display(Name = "LinkoExchange term")]
        public DropdownOptionViewModel Parameter { get; set; }

        public override DropdownOptionViewModel TranslatedItem
        {
            get { return Parameter; }
            set { Parameter = value; }
        }

        #endregion
    }

    public class DataSourceUnitTranslationViewModel : DataSourceTranslationViewModel
    {
        #region public properties

        [UIHint(uiHint:"AuthorityUnitEditor")]
        [Display(Name = "LinkoExchange term")]
        public DropdownOptionViewModel Unit { get; set; }

        public override DropdownOptionViewModel TranslatedItem
        {
            get { return Unit; }
            set { Unit = value; }
        }

        #endregion
    }
}