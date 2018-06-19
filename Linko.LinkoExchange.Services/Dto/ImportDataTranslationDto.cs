using System.Collections.Generic;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ImportDataTranslationDto
    {
        #region public properties

        public SampleImportColumnName SampleImportColumnName { get; set; }
        public List<ListItemDto> Options { get; set; }
        public List<string> MissingTranslations { get; set; }
        public List<DataSourceTranslationDto> ExistingDataTranslations { get; set; }

        #endregion
    }
}