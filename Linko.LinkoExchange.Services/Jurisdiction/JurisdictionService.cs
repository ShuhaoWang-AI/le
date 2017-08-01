using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Linko.LinkoExchange.Services.Jurisdiction
{
    public class JurisdictionService : IJurisdictionService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IMapHelper _mapHelper;
        private readonly ILogger _logService;

        public JurisdictionService(LinkoExchangeContext dbContext, IMapHelper mapHelper, ILogger logService)
        {
            _dbContext = dbContext;
            _mapHelper = mapHelper;
            _logService = logService;
        }

        public ICollection<JurisdictionDto> GetStateProvs(int countryId)
        {
            _logService.Info($"Entering GetStateProvs. countryId={countryId}");

            List<JurisdictionDto> dtos = new List<JurisdictionDto>();
            var states = _dbContext.Jurisdictions
                .Where(j => j.CountryId == countryId);

            if (states != null)
            {
                foreach (var state in states)
                {
                    dtos.Add(_mapHelper.GetJurisdictionDtoFromJurisdiction(state));
                }
            }

            _logService.Info($"Exiting GetStateProvs. countryId={countryId}");

            return dtos;
        }

        public JurisdictionDto GetJurisdictionById(int jurisdictionId)
        {
            var jurisdiction = _dbContext.Jurisdictions
                .SingleOrDefault(j => j.JurisdictionId == jurisdictionId);

            if (jurisdiction != null)
            {
                JurisdictionDto dto = _mapHelper.GetJurisdictionDtoFromJurisdiction(jurisdiction);
                return dto;
            }
            else
            {
                var errorMessage = $"GetJurisdictionById. Could not find Jurisdiction associated with JurisdictionId={jurisdictionId}.";
                _logService.Info(errorMessage);
                throw new Exception(errorMessage);
            }
        }
    }
}
