namespace Linko.LinkoExchange.Services.Report.DataXML
{
    public class ReportHeader
    {
        #region public properties

        public string ReportName { get; set; }
        public string ReportPeriodStartDateTimeUtc { get; set; }
        public string ReportPeriodEndDateTimeUtc { get; set; }
        public string ReportSubmissionDateUtc { get; set; }
        public string AuthorityTimeZone { get; set; }

        #endregion
    }
}