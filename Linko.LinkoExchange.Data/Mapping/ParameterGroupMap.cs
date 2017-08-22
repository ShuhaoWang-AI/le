using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ParameterGroupMap : EntityTypeConfiguration<ParameterGroup>
    {
        #region constructors and destructor

        public ParameterGroupMap()
        {
            ToTable(tableName:"tParameterGroup");

            HasKey(x => x.ParameterGroupId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:100);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.ParameterGroups)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.IsActive).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}