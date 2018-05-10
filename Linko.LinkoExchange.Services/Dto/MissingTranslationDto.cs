using System.Collections.Generic;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Dto
{
    public class MissingTranslationDto
    {
        #region public properties

        public SystemFieldName SystemFieldName { get; set; }
        public List<CustomSelectListItemDto> SelectListItems { get; set; }
        public List<string> MissingTranslations { get; set; }

        #endregion
    }
}