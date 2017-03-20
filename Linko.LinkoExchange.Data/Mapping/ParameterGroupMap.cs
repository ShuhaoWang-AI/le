using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ParameterGroupMap : EntityTypeConfiguration<ParameterGroup>
    {
        public ParameterGroupMap()
        {
            ToTable("tParameterGroup");

            HasKey(x => x.ParameterGroupId);

            Property(x => x.Name).IsRequired().HasMaxLength(100);

            Property(x => x.Description).IsOptional().HasMaxLength(500);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.ParameterGroups)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);

            Property(x => x.IsActive).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}