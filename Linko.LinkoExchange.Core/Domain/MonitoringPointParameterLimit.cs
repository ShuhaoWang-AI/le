namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a limit for a certain Parameter in a Monitoring Point.
    /// </summary>
    public class MonitoringPointParameterLimit
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int MonitoringPointParameterLimitId { get; set; }

        /// <summary>
        ///     Unique for a certain Limit Type and Limit Basis.
        /// </summary>
        public int MonitoringPointParameterId { get; set; }

        public virtual MonitoringPointParameter MonitoringPointParameter { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        ///     The lower range of a limit, i.e. pH range is 5.5 - 10.5. This would be 5.5.
        /// </summary>
        public double? MinimumValue { get; set; }

        /// <summary>
        ///     The upper range of a range or just the limit.
        /// </summary>
        public double MaximumValue { get; set; }

        public int BaseUnitId { get; set; }
        public virtual Unit BaseUnit { get; set; }

        public int? CollectionMethodTypeId { get; set; }
        public virtual CollectionMethodType CollectionMethodType { get; set; }

        public int LimitTypeId { get; set; }
        public virtual LimitType LimitType { get; set; }

        public int LimitBasisId { get; set; }
        public virtual LimitBasis LimitBasis { get; set; }

        public bool IsAlertsOnly { get; set; }

        #endregion
    }
}