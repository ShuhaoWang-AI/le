using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class RegulatoryProgramMap : EntityTypeConfiguration<RegulatoryProgram>
    {
        public RegulatoryProgramMap()
        {
            ToTable("tRegulatoryProgram");

            HasKey(x => x.RegulatoryProgramId);

            Property(x => x.Name).IsRequired().HasMaxLength(100);

            Property(x => x.Description).IsOptional().HasMaxLength(500);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}