using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a permitted module within a regulatory program for a particular organization.
    /// </summary>
    public class OrganizationRegulatoryProgramModule
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int OrganizationRegulatoryProgramModuleId { get; set; }

        public int OrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }

        public int ModuleId { get; set; }
        public virtual Module Module { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        #endregion
    }
}