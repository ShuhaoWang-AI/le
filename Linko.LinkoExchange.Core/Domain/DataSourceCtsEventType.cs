namespace Linko.LinkoExchange.Core.Domain
{
    public class DataSourceCtsEventType
    {
        #region public properties

        public int DataSourceCtsEventTypeId { get; set; }

        public string DataSourceTerm { get; set; }

        public int DataSourceId { get; set; }
        public virtual DataSource DataSource { get; set; }

        public int CtsEventTypeId { get; set; }
        public virtual CtsEventType CtsEventType { get; set; }

        #endregion
    }
}
