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
        private readonly ILogger _logger;
        private readonly IMapHelper _mapHelper;

        #endregion

        #region constructors and destructor

        public JurisdictionService(LinkoExchangeContext dbContext, IMapHelper mapHelper, ILogger logger)
        {
            _dbContext = dbContext;
            _mapHelper = mapHelper;
            _logger = logger;
        }

        #endregion

        #region interface implementations

        public ICollection<JurisdictionDto> GetStateProvs(int countryId)
        {
            _logger.Info(message:$"Start: JurisdictionService.GetStateProvs. countryId={countryId}");

            var dtos = new List<JurisdictionDto>();
            var states = _dbContext.Jurisdictions.Where(j => j.CountryId == countryId);

            foreach (var state in states)
            {
                dtos.Add(item:_mapHelper.GetJurisdictionDtoFromJurisdiction(jurisdiction:state));
            }

            _logger.Info(message:$"End: JurisdictionService.GetStateProvs.");

            return dtos;
        }

        public JurisdictionDto GetJurisdictionById(int? jurisdictionId)
        {
            if (jurisdictionId.HasValue == false)
            {
                return null;
            }

            var jurisdiction = _dbContext.Jurisdictions.SingleOrDefault(j => j.JurisdictionId == jurisdictionId);

            if (jurisdiction != null)
            {
                var dto = _mapHelper.GetJurisdictionDtoFromJurisdiction(jurisdiction:jurisdiction);
                return dto;
            }
            else
            {
                var errorMessage = $"JurisdictionService.GetJurisdictionById. Could not find Jurisdiction associated with JurisdictionId={jurisdictionId}.";
                throw new Exception(message:errorMessage);
            }
        }

        #endregion
    }
}