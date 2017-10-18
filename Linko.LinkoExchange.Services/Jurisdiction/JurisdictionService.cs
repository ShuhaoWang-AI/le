using System;
using System.Collections.Generic;
using System.Linq;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using NLog;

namespace Linko.LinkoExchange.Services.Jurisdiction
{
    public class JurisdictionService : IJurisdictionService
    {
        #region fields

        private readonly LinkoExchangeContext _dbContext;
        private readonly ILogger _logService;
        private readonly IMapHelper _mapHelper;

        #endregion

        #region constructors and destructor

        public JurisdictionService(LinkoExchangeContext dbContext, IMapHelper mapHelper, ILogger logService)
        {
            _dbContext = dbContext;
            _mapHelper = mapHelper;
            _logService = logService;
        }

        #endregion

        #region interface implementations

        public ICollection<JurisdictionDto> GetStateProvs(int countryId)
        {
            _logService.Info(message:$"Entering GetStateProvs. countryId={countryId}");

            var dtos = new List<JurisdictionDto>();
            var states = _dbContext.Jurisdictions.Where(j => j.CountryId == countryId);

            foreach (var state in states)
            {
                dtos.Add(item:_mapHelper.GetJurisdictionDtoFromJurisdiction(jurisdiction:state));
            }

            _logService.Info(message:$"Exiting GetStateProvs. countryId={countryId}");

            return dtos;
        }

        public JurisdictionDto GetJurisdictionById(int? jurisdictionId)
        {
            if (jurisdictionId.HasValue == false)
            {
                return null;
            }

            var jurisdiction = _dbContext.Jurisdictions
                                         .SingleOrDefault(j => j.JurisdictionId == jurisdictionId);

            if (jurisdiction != null)
            {
                var dto = _mapHelper.GetJurisdictionDtoFromJurisdiction(jurisdiction:jurisdiction);
                return dto;
            }
            else
            {
                var errorMessage = $"GetJurisdictionById. Could not find Jurisdiction associated with JurisdictionId={jurisdictionId}.";
                _logService.Info(message:errorMessage);
                throw new Exception(message:errorMessage);
            }
        }

        #endregion
    }
}