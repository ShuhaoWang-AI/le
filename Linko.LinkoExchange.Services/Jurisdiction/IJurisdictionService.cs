using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Jurisdiction
{
    public interface IJurisdictionService
    {
        /// <summary>
        ///     Returns a collection of state/provinces associated with a country.
        /// </summary>
        /// <param name="countryId"> </param>
        /// <returns> </returns>
        ICollection<JurisdictionDto> GetStateProvs(int countryId);

        /// <summary>
        ///     Returns a jurisdiction object associated with an Id. Throws an exception if one could not be found.
        /// </summary>
        /// <param name="jurisdictionId"> </param>
        /// <returns> </returns>
        JurisdictionDto GetJurisdictionById(int? jurisdictionId);
    }
}