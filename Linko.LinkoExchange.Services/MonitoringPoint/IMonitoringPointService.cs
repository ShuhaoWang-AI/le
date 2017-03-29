using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.MonitoringPoint
{
    public interface IMonitoringPointService
    {
        IEnumerable<MonitoringPointDto> GetMonitoringPoints();
        MonitoringPointDto GetMonitoringPoint(int monitoringPointId);
    }
}
