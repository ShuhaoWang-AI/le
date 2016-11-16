using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.TimeZone
{
    public interface ITimeZoneService
    {
        string GetTimeZoneName(int timeZoneId);
        ICollection<TimeZoneDto> GetTimeZones();
    }
}
