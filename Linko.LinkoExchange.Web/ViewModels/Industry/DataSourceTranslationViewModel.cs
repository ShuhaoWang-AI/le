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
                default: throw new NotImplementedException(message:$"DataSourceTranslationType {translationType} is unsupported");
            }
        }
    }

    public class DataSourceMonitoringPointTranslationViewModel : DataSourceTranslationViewModel
    {
        #region public properties

        [UIHint(uiHint:"AuthorityMonitoringPointEditor")]
        [Display(Name = "Choose which Monitoring Point this is")]
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
        [Display(Name = "Choose which Sample Type this is")]
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
        [Display(Name = "Choose which Collection Method this is")]
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
        [Display(Name = "Choose which Parameter this is")]
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
        [Display(Name = "Choose which Unit this is")]
        public DropdownOptionViewModel Unit { get; set; }

        public override DropdownOptionViewModel TranslatedItem
        {
            get { return Unit; }
            set { Unit = value; }
        }

        #endregion
    }
}