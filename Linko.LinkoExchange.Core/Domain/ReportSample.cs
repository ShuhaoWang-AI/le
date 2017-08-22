namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents an included Sample for a Report Package Element Type.
    /// </summary>
    public class ReportSample
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int ReportSampleId { get; set; }

        public int ReportPackageElementTypeId { get; set; }
        public virtual ReportPackageElementType ReportPackageElementType { get; set; }

        public int SampleId { get; set; }
        public virtual Sample Sample { get; set; }

        #endregion
    }
}