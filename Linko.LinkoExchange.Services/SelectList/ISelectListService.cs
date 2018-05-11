using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.SelectList
{
    public interface ISelectListService
    {
        IEnumerable<ListItemDto> GetIndustryMonitoringPointSelectList();
        IEnumerable<ListItemDto> GetAuthoritySampleTypeSelectList();
        IEnumerable<ListItemDto> GetAuthorityCollectionMethodSelectList();
        IEnumerable<ListItemDto> GetAuthorityParameterSelectList();
        IEnumerable<ListItemDto> GetAuthorityUnitSelectList();
    }
}