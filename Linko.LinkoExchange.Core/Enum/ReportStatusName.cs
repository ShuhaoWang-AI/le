
namespace Linko.LinkoExchange.Core.Enum
{
    public enum ReportStatusName
    {
        Draft,                      // for Industry -- actual entry in tReportStatus
        ReadyToSubmit,              // for Industry -- actual entry in tReportStatus
        Submitted,                  // for Industry -- actual entry in tReportStatus
        Repudiated,                 // for Industry -- actual entry in tReportStatus
        SubmittedPendingReview,     // for Authority -- only used to filter for grid display
        RepudiatedPendingReview,    // for Authority -- only used to filter for grid display
        All                         // for Authority -- only used to filter for grid display
    }
}
