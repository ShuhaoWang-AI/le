using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class OrganizationTypeRegulatoryProgramMap : EntityTypeConfiguration<OrganizationTypeRegulatoryProgram>
    {
        public OrganizationTypeRegulatoryProgramMap()
        {
            ToTable("tOrganizationTypeRegulatoryProgram");

            HasKey(x => x.OrganizationTypeRegulatoryProgramId);

            HasRequired(a => a.RegulatoryProgram)
                .WithMany(b => b.OrganizationTypeRegulatoryPrograms)
                .HasForeignKey(c => c.RegulatoryProgramId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.OrganizationType)
                .WithMany(b => b.OrganizationTypeRegulatoryPrograms)
                .HasForeignKey(c => c.OrganizationTypeId)
                .WillCascadeOnDelete(false);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}