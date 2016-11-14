using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Jurisdiction
{
    public interface IJurisdictionService
    {
        ICollection<JurisdictionDto> GetStateProvs(int countryId);
        JurisdictionDto GetJurisdictionById(int jurisdictionId);
    }
}
