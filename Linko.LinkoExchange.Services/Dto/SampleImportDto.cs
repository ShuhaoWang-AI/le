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
        public List<FileCellObject> FileCells { get; set; }

        #endregion
    }

    public class FileCellObject
    {
        #region public properties

        public int RowNumber { get; set; }
        public SystemFieldName ColumnName { get; set; }
        public string OriginalValue { get; set; }
        public dynamic TranslatedValue { get; set; }
        public int? TranslatedValueId { get; set; }

        #endregion
    }
}