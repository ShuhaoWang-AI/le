using Linko.LinkoExchange.Services.Dto;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Jurisdiction
{
    public interface IJurisdictionService
    {
        ICollection<JurisdictionDto> GetStateProvs(int countryId);
        JurisdictionDto GetJurisdictionById(int jurisdictionId);
    }
}
