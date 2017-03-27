namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents an attached file for a Report Package Element Type.
    /// </summary>
    public partial class ReportFile
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int ReportFileId { get; set; }

        public int ReportPackageElementTypeId { get; set; }
        public virtual ReportPackageElementType ReportPackageElementType { get; set; }

        public int FileStoreId { get; set; }
        public virtual FileStore FileStore { get; set; }
    }
}
