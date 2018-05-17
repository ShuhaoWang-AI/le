namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents File Version Template Field.
    /// </summary>
    public class FileVersionTemplateField
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int FileVersionTemplateFieldId { get; set; }

        public int FileVersionTemplateId { get; set; }
        public virtual FileVersionTemplate FileVersionTemplate { get; set; }

        public int SystemFieldId { get; set; }
        public virtual SystemField SystemField { get; set; }

        #endregion
    }
}