using AutoMapper;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.TimeZone
{
    public class TimeZoneService : ITimeZoneService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IMapper _mapper;

        public TimeZoneService(LinkoExchangeContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public string GetTimeZoneName(int timeZoneId)
        {
            return (_dbContext.TimeZones.Single(t => t.TimeZoneId == timeZoneId).Name);
        }

        public ICollection<TimeZoneDto> GetTimeZones()
        {
            var dtos = _mapper.Map<IEnumerable<Core.Domain.TimeZone>, ICollection<TimeZoneDto>>(_dbContext.TimeZones);
            return dtos;
        }
    }
}
