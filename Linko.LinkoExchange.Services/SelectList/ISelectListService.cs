using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.SelectList
{
    public interface ISelectListService
    {
        List<ListItemDto> GetSelectList(SelectListType selectListType, bool withEmptyItem = false);
        List<ListItemDto> GetIndustryMonitoringPointSelectList(bool withEmptyItem = false);
        List<ListItemDto> GetAuthoritySampleTypeSelectList(bool withEmptyItem = false);
        List<ListItemDto> GetAuthorityCollectionMethodSelectList(bool withEmptyItem = false);
        List<ListItemDto> GetAuthorityParameterSelectList(bool withEmptyItem = false);
        List<ListItemDto> GetAuthorityUnitSelectList(bool withEmptyItem = false);
    }

    public enum SelectListType
    {
        IndustryMonitoringPoint,
        AuthorityCollectionMethod,
        AuthoritySampleType,
        AuthorityParameter,
        AuthorityUnit
    }
}