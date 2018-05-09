namespace Linko.LinkoExchange.Core.Domain
{
    public class DataSourceUnit
    {
        #region public properties

        public int DataSourceUnitId { get; set; }

        public string DataSourceTerm { get; set; }

        public int DataSourceId { get; set; }

        public int UnitId { get; set; }
        public virtual DataSource DataSource { get; set; }

        public virtual Unit Unit { get; set; }

        #endregion
    }
}
