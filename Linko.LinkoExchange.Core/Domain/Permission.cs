using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a specific permission.
    /// </summary>
    public partial class Permission
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int PermissionId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int ModuleId { get; set; }
        public virtual Module Module { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }
    }
}
