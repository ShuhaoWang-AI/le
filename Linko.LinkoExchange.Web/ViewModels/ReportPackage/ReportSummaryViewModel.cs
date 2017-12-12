using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Linko.LinkoExchange.Web.ViewModels.ReportPackage
{
    public class ReportSummaryViewModel
    {
        public List<ReportContentReviewItem> ReportContentReviewItems { get; set; }
        public int RequiredItemCount
        {
            get
            {
                if (this.ReportContentReviewItems != null)
                {
                    return this.ReportContentReviewItems.Count(item => item.IsRequired);
                }
                else
                {
                    return -1;
                }
            }
        }
        public int RequiredAndPresentItemCount
        {
            get
            {
                if (this.ReportContentReviewItems != null)
                {
                    return this.ReportContentReviewItems.Count(item => item.IsRequired && item.IsPresentInReport);
                }
                else
                {
                    return -1;
                }
            }
        }
        public int SampleResultsInComplianceCount { get; set; }
        public int SampleResultsNonComplianceCount { get; set; }

        public List<SamplingRequirementsItem> SamplingRequirementsItems { get; set; }
    }

    public class ReportContentReviewItem
    {
        public string ReportElementName { get; set; }
        public bool IsRequired { get; set; }
        public bool IsPresentInReport { get; set; }
    }

    public class SamplingRequirementsItem
    {
        public string MonitoringPointName { get; set; }
        public string ParameterName { get; set; }
        public int ExpectedSampleCount { get; set; }
        public int IncludedSampleCount { get; set; }
    }
}