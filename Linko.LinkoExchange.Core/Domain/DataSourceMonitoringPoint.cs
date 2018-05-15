namespace Linko.LinkoExchange.Core.Domain
{
    public class DataSourceMonitoringPoint
    {
        #region public properties

        public int DataSourceMonitoringPointId { get; set; }

        public string DataSourceTerm { get; set; }

        public int DataSourceId { get; set; }
        public virtual DataSource DataSource { get; set; }

        public int MonitoringPointId { get; set; }
        public virtual MonitoringPoint MonitoringPoint { get; set; }

        #endregion
    }
}
