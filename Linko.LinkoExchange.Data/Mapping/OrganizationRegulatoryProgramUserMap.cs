using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public partial class OrganizationRegulatoryProgramUserMap : EntityTypeConfiguration<OrganizationRegulatoryProgramUser>
    {
        public OrganizationRegulatoryProgramUserMap()
        {
            ToTable("tOrganizationRegulatoryProgramUser");

            HasKey(x => x.OrganizationRegulatoryProgramUserId);

            Property(x => x.UserProfileId).IsRequired();
            //HasRequired(a => a.UserProfile)
            //    .WithMany()
            //    .HasForeignKey(c => c.UserProfileId)
            //    .WillCascadeOnDelete(false);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.OrganizationRegulatoryProgramUsers)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.InviterOrganizationRegulatoryProgram)
                .WithMany(b => b.InviterOrganizationRegulatoryProgramUsers)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);

            HasOptional(a => a.PermissionGroup)
                .WithMany(b => b.OrganizationRegulatoryProgramUsers)
                .HasForeignKey(c => c.PermissionGroupId)
                .WillCascadeOnDelete(false);

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
    }
}