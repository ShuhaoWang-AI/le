using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a setting name.
    /// </summary>
    public class SettingTemplate
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int SettingTemplateId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        ///     Default value to be copied over during the initial specific Organization or OrganizationRegulatoryProgram setup.
        /// </summary>
        public string DefaultValue { get; set; }

        public int OrganizationTypeId { get; set; }
        public virtual OrganizationType OrganizationType { get; set; }

        /// <summary>
        ///     NULL: the setting is applicable to all regulatory program for a particular organization type.
        /// </summary>
        public int? RegulatoryProgramId { get; set; }

        public virtual RegulatoryProgram RegulatoryProgram { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        // Reverse navigation
        public virtual ICollection<OrganizationRegulatoryProgramSetting> OrganizationRegulatoryProgramSettings { get; set; }

        public virtual ICollection<OrganizationSetting> OrganizationSettings { get; set; }

        #endregion
    }
}