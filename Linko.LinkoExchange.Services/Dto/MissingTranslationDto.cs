using System.Collections.Generic;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Dto
{
    public class MissingTranslationDto
    {
        #region public properties

        public SampleImportColumnName SampleImportColumnName { get; set; }
        public List<ListItemDto> Options { get; set; }
        public List<string> MissingTranslations { get; set; }
        public string Title { get; set; }

        #endregion
    }
}