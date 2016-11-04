using Linko.LinkoExchange.Data;
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

        public TimeZoneService(LinkoExchangeContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string GetTimeZoneName(int timeZoneId)
        {
            return (_dbContext.TimeZones.Single(t => t.TimeZoneId == timeZoneId).Name);
        }
    }
}
