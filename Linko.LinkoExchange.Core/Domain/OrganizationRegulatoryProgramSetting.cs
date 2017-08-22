using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a setting within a regulatory program for a particular organization.
    ///     A regulatee will inherit these settings from its regulator.
    /// </summary>
    public class OrganizationRegulatoryProgramSetting
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int OrganizationRegulatoryProgramSettingId { get; set; }

        public int OrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }

        public int SettingTemplateId { get; set; }
        public virtual SettingTemplate SettingTemplate { get; set; }

        public string Value { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        #endregion
    }
}