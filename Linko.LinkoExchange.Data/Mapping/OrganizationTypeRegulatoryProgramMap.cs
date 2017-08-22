using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class OrganizationTypeRegulatoryProgramMap : EntityTypeConfiguration<OrganizationTypeRegulatoryProgram>
    {
        #region constructors and destructor

        public OrganizationTypeRegulatoryProgramMap()
        {
            ToTable(tableName:"tOrganizationTypeRegulatoryProgram");

            HasKey(x => x.OrganizationTypeRegulatoryProgramId);

            HasRequired(a => a.RegulatoryProgram)
                .WithMany(b => b.OrganizationTypeRegulatoryPrograms)
                .HasForeignKey(c => c.RegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.OrganizationType)
                .WithMany(b => b.OrganizationTypeRegulatoryPrograms)
                .HasForeignKey(c => c.OrganizationTypeId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}