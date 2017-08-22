using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.MonitoringPoint
{
    public interface IMonitoringPointService
    {
        IEnumerable<MonitoringPointDto> GetMonitoringPoints();
        MonitoringPointDto GetMonitoringPoint(int monitoringPointId);
    }
}