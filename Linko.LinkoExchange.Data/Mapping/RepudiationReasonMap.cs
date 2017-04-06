using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public partial class RepudiationReasonMap : EntityTypeConfiguration<RepudiationReason>
    {
        public RepudiationReasonMap()
        {
            ToTable("tRepudiationReason");

            HasKey(x => x.RepudiationReasonId);

            Property(x => x.Name).IsRequired().HasMaxLength(100);

            Property(x => x.Description).IsOptional().HasMaxLength(500);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.RepudiationReasons)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}