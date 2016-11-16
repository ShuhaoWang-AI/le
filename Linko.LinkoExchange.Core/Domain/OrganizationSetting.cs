using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a setting for a particular organization.
    /// </summary>
    public partial class OrganizationSetting
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int OrganizationSettingId { get; set; }

        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }

        public int SettingTemplateId { get; set; }
        public virtual SettingTemplate SettingTemplate { get; set; }

        public string Value { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }
    }
}