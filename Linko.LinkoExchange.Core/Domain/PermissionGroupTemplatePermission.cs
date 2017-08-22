namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a specific permission within a system-default permission group.
    /// </summary>
    public class PermissionGroupTemplatePermission
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int PermissionGroupTemplatePermissionId { get; set; }

        public int PermissionGroupTemplateId { get; set; }
        public virtual PermissionGroupTemplate PermissionGroupTemplate { get; set; }

        public int PermissionId { get; set; }
        public virtual Permission Permission { get; set; }

        #endregion
    }
}