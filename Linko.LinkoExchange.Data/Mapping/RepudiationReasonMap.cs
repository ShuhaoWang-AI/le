using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class RepudiationReasonMap : EntityTypeConfiguration<RepudiationReason>
    {
        #region constructors and destructor

        public RepudiationReasonMap()
        {
            ToTable(tableName:"tRepudiationReason");

            HasKey(x => x.RepudiationReasonId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:100);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.RepudiationReasons)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}