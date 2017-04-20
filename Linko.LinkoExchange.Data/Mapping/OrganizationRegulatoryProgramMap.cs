using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class OrganizationRegulatoryProgramMap : EntityTypeConfiguration<OrganizationRegulatoryProgram>
    {
        public OrganizationRegulatoryProgramMap()
        {
            ToTable("tOrganizationRegulatoryProgram");

            HasKey(x => x.OrganizationRegulatoryProgramId);

            HasRequired(a => a.RegulatoryProgram)
                .WithMany(b => b.OrganizationRegulatoryPrograms)
                .HasForeignKey(c => c.RegulatoryProgramId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.Organization)
                .WithMany(b => b.OrganizationRegulatoryPrograms)
                .HasForeignKey(c => c.OrganizationId)
                .WillCascadeOnDelete(false);

            HasOptional(a => a.RegulatorOrganization)
                .WithMany(b => b.RegulatorOrganizationRegulatoryPrograms)
                .HasForeignKey(c => c.RegulatorOrganizationId)
                .WillCascadeOnDelete(false);

            Property(x => x.AssignedTo).IsOptional().HasMaxLength(50);

            Property(x => x.ReferenceNumber).IsOptional().HasMaxLength(50);

            Property(x => x.IsEnabled).IsRequired();

            Property(x => x.IsRemoved).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}