namespace Linko.LinkoExchange.Core.Domain
{
    public class DataSourceParameter
    {
        #region public properties

        public int DataSourceParameterId { get; set; }

        public string DataSourceTerm { get; set; }

        public int DataSourceId { get; set; }

        public int ParameterId { get; set; }

        public virtual DataSource DataSource { get; set; }

        public virtual Parameter Parameter { get; set; }

        #endregion
    }
}
