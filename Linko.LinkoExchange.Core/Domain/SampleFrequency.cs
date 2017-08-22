namespace Linko.LinkoExchange.Core.Domain
{
    public class SampleFrequency
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int SampleFrequencyId { get; set; }

        public int MonitoringPointParameterId { get; set; }
        public virtual MonitoringPointParameter MonitoringPointParameter { get; set; }

        /// <summary>
        ///     Unique within a particular MonitoringPointParameterId.
        /// </summary>
        public int CollectionMethodId { get; set; }

        public virtual CollectionMethod CollectionMethod { get; set; }

        public string IUSampleFrequency { get; set; }

        public string AuthoritySampleFrequency { get; set; }

        #endregion
    }
}