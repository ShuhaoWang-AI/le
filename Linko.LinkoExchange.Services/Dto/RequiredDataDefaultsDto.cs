using System.Collections.Generic;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Dto {
    public class RequiredDataDefaultsDto
    {
        public SampleImportColumnName SampleImportColumnName { get; set; }
        public List<ListItemDto> Options { get; set; }
        
    }
}