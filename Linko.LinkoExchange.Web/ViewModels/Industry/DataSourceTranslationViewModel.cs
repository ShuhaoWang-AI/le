using System;
using System.ComponentModel.DataAnnotations;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Web.ViewModels.Shared;

namespace Linko.LinkoExchange.Web.ViewModels.Industry
{
    public abstract class DataSourceTranslationViewModel
    {
        #region public properties

        [ScaffoldColumn(scaffold: false)]
        public int? Id { get; set; }

        public int DataSourceId { get; set; }

        [Display(Name = "This was in the file")]
        public string DataSourceTerm { get; set; }

        public DataSourceTranslationType TranslationType { get; set; }

        public abstract DropdownOptionViewModel TranslatedItem { get; set; }

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
                default: throw new NotImplementedException(message: "DataSourceType is unsupported");
            }
        }
    }

    public class DataSourceMonitoringPointTranslationViewModel : DataSourceTranslationViewModel
    {
        #region public properties

        [UIHint(uiHint: "AuthorityMonitoringPointEditor")]
        [Display(Name = "Choose which Monitoring Point this is")]
        public override DropdownOptionViewModel TranslatedItem { get; set; }

        #endregion
    }

    public class DataSourceSampleTypeTranslationViewModel : DataSourceTranslationViewModel
    {
        #region public properties

        [UIHint(uiHint: "AuthoritySampleTypeEditor")]
        [Display(Name = "Choose which Sample Type this is")]
        public override DropdownOptionViewModel TranslatedItem { get; set; }

        #endregion
    }

    public class DataSourceCollectionMethodTranslationViewModel : DataSourceTranslationViewModel
    {
        #region public properties

        [UIHint(uiHint: "AuthorityCollectionMethodEditor")]
        [Display(Name = "Choose which Collection Method this is")]
        public override DropdownOptionViewModel TranslatedItem { get; set; }

        #endregion
    }

    public class DataSourceParameterTranslationViewModel : DataSourceTranslationViewModel
    {
        #region public properties

        [UIHint(uiHint: "AuthorityParameterEditor")]
        [Display(Name = "Choose which Parameter this is")]
        public override DropdownOptionViewModel TranslatedItem { get; set; }

        #endregion
    }

    public class DataSourceUnitTranslationViewModel : DataSourceTranslationViewModel
    { 
        #region public properties

        [UIHint(uiHint: "AuthorityUnitEditor")]
        [Display(Name = "Choose which Unit this is")]
        public override DropdownOptionViewModel TranslatedItem { get; set; }

        #endregion
    }
}
