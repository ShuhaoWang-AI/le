using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class OrganizationRegulatoryProgramUserMap : EntityTypeConfiguration<OrganizationRegulatoryProgramUser>
    {
        #region constructors and destructor

        public OrganizationRegulatoryProgramUserMap()
        {
            ToTable(tableName:"tOrganizationRegulatoryProgramUser");

            HasKey(x => x.OrganizationRegulatoryProgramUserId);

            Property(x => x.UserProfileId).IsRequired();

            //HasRequired(a => a.UserProfile)
            //    .WithMany()
            //    .HasForeignKey(c => c.UserProfileId)
            //    .WillCascadeOnDelete(false);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.OrganizationRegulatoryProgramUsers)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.InviterOrganizationRegulatoryProgram)
                .WithMany(b => b.InviterOrganizationRegulatoryProgramUsers)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            HasOptional(a => a.PermissionGroup)
                .WithMany(b => b.OrganizationRegulatoryProgramUsers)
                .HasForeignKey(c => c.PermissionGroupId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.RegistrationDateTimeUtc).IsRequired();

            Property(x => x.IsRegistrationApproved).IsRequired();

            Property(x => x.IsRegistrationDenied).IsRequired();

            Property(x => x.IsEnabled).IsRequired();

            Property(x => x.IsRemoved).IsRequired();

            Property(x => x.IsSignatory).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}