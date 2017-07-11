using Linko.LinkoExchange.Services.Dto;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.MonitoringPoint
{
    public interface IMonitoringPointService
    {
        IEnumerable<MonitoringPointDto> GetMonitoringPoints();
        MonitoringPointDto GetMonitoringPoint(int monitoringPointId);
    }
}
