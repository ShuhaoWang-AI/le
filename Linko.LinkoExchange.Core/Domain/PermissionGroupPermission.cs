namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a specific permission within a permission group.
    /// </summary>
    public class PermissionGroupPermission
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int PermissionGroupPermissionId { get; set; }

        public int PermissionGroupId { get; set; }
        public virtual PermissionGroup PermissionGroup { get; set; }

        public int PermissionId { get; set; }
        public virtual Permission Permission { get; set; }

        #endregion
    }
}