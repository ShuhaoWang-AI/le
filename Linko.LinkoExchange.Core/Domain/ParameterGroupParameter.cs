namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a specific Parameter within a static Parameter Group.
    /// </summary>
    public partial class ParameterGroupParameter
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int ParameterGroupParameterId { get; set; }

        public int ParameterGroupId { get; set; }
        public virtual ParameterGroup ParameterGroup { get; set; }

        public int ParameterId { get; set; }
        public virtual Parameter Parameter { get; set; }
    }
}
