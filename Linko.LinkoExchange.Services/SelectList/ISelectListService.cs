using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.SelectList
{
    public interface ISelectListService
    {
        List<ListItemDto> GetIndustryMonitoringPointSelectList(bool withEmptyItem = false);
        List<ListItemDto> GetAuthoritySampleTypeSelectList(bool withEmptyItem = false);
        List<ListItemDto> GetAuthorityCollectionMethodSelectList(bool withEmptyItem = false);
        List<ListItemDto> GetAuthorityParameterSelectList(bool withEmptyItem = false);
        List<ListItemDto> GetAuthorityUnitSelectList(bool withEmptyItem = false);
    }
}