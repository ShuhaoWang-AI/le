using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Linko.LinkoExchange.Core.Domain
{
    public class OrganizationRegulatoryProgramUser
    {
        [Key]
        public int OrganizationRegulatoryProgramUserId { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; } 
        public bool IsRegistrationApproved { get; set; }
        public bool IsRegistrationDenied { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsRemoved { get; set; }
        public bool IsSignatory { get; set; }
        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }
        public int? LastModifierUserId { get; set; } 
        public int UserProfileId { get;set; }
        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }
        public virtual PermissionGroup PermissionGroup { get; set; }
        public int? PermissionGroupId { get; set; }
        public DateTimeOffset? CreationDateTimeUtc { get;set; }
        public DateTimeOffset? RegistrationDateTimeUtc { get;set;}
    }
}
