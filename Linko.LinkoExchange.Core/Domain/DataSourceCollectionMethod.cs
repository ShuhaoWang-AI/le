namespace Linko.LinkoExchange.Core.Domain
{
    public class DataSourceCollectionMethod
    {
        #region public properties

        public int DataSourceCollectionMethodId { get; set; }

        public string DataSourceTerm { get; set; }

        public int DataSourceId { get; set; }

        public int CollectionMethodId { get; set; }

        public virtual DataSource DataSource { get; set; }

        public virtual CollectionMethod CollectionMethod { get; set; }

        #endregion
    }
}
