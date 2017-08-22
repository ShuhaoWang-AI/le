using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class CtsEventTypeMap : EntityTypeConfiguration<CtsEventType>
    {
        #region constructors and destructor

        public CtsEventTypeMap()
        {
            ToTable(tableName:"tCtsEventType");

            HasKey(x => x.CtsEventTypeId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:50);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            Property(x => x.CtsEventCategoryName).IsRequired().HasMaxLength(value:50);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.CtsEventTypes)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.IsEnabled).IsRequired();

            Property(x => x.IsRemoved).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}