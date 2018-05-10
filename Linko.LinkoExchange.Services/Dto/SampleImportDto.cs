using System.Collections.Generic;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Dto
{
    public class SampleImportDto
    {
        #region public properties

        public ImportTempFileDto TempFile { get; set; }
        public DataSourceDto DataSource { get; set; }
        public FileVersionDto FileVersion { get; set; }
        public List<FileRowObject> FileRows { get; set; }

        #endregion
    }

    public class FileRowObject
    {
        #region public properties

        public int RowNumber { get; set; }
        public SystemFieldName SystemFieldName { get; set; }
        public string OriginalValue { get; set; }
        public dynamic TranslatedValue { get; set; }
        public int? TranslatedValueId { get; set; }

        #endregion
    }
}