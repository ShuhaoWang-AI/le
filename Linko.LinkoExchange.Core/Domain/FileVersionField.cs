using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents File Version Field.
    /// </summary>
    public class FileVersionField
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int FileVersionFieldId { get; set; }
        
        public string Name { get; set; }

        public string Description { get; set; }
        
        public int FileVersionId { get; set; }
        public virtual FileVersion FileVersion { get; set; }

        public int SystemFieldId { get; set; }
        public virtual SystemField SystemField { get; set; }
        
        public int DataOptionalityId { get; set; }
        public virtual DataOptionality DataOptionality { get; set; }

        public int? Size { get; set; }

        public string ExampleData { get; set; }

        public string AdditionalComments { get; set; }

        #endregion
    }
}