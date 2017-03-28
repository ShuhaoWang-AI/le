namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a file data.
    /// 
    /// The binary data for the file is stored separately from other details. This gives us multiple benefits:
    /// - enables us to put this table into different filegroup and hence, easier to maintain because the filegroup can be put onto different disk volume.
    /// - improves the speed of retrieving other details because LOB column is not retrieved at the same time.
    /// </summary>
    public partial class FileStoreData
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int FileStoreId { get; set; }
        public virtual FileStore FileStore { get; set; }

        public byte[] Data { get; set; }
    }
}

