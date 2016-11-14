using AutoMapper;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
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
        private readonly IMapper _mapper;

        public JurisdictionService(LinkoExchangeContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public ICollection<JurisdictionDto> GetStateProvs(int countryId)
        {
            List<JurisdictionDto> dtos = new List<JurisdictionDto>();
            var states = _dbContext.Jurisdictions.Where(j => j.CountryId == countryId);
            if (states != null)
                foreach (var state in states)
                {
                    dtos.Add(_mapper.Map<Core.Domain.Jurisdiction, JurisdictionDto>(state));
                }


            return dtos;
        }

        public JurisdictionDto GetJurisdictionById(int jurisdictionId)
        {
            return _mapper.Map<Core.Domain.Jurisdiction, JurisdictionDto>(_dbContext.Jurisdictions.Single(j => j.JurisdictionId == jurisdictionId));
        }
    }
}
