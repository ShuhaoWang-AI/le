using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Jurisdiction
{
    public class JurisdictionService : IJurisdictionService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IMapHelper _mapHelper;

        public JurisdictionService(LinkoExchangeContext dbContext, IMapHelper mapHelper)
        {
            _dbContext = dbContext;
            _mapHelper = mapHelper;
        }

        public ICollection<JurisdictionDto> GetStateProvs(int countryId)
        {
            List<JurisdictionDto> dtos = new List<JurisdictionDto>();
            var states = _dbContext.Jurisdictions.Where(j => j.CountryId == countryId);
            if (states != null)
                foreach (var state in states)
                {
                    JurisdictionDto dto = _mapHelper.GetJurisdictionDtoFromJurisdiction(state);

                    dtos.Add(dto);

                }


            return dtos;
        }

        public JurisdictionDto GetJurisdictionById(int jurisdictionId)
        {
            Core.Domain.Jurisdiction jurisdiction = _dbContext.Jurisdictions.Single(j => j.JurisdictionId == jurisdictionId);
            JurisdictionDto dto = _mapHelper.GetJurisdictionDtoFromJurisdiction(jurisdiction);

            return dto;
        }
    }
}
