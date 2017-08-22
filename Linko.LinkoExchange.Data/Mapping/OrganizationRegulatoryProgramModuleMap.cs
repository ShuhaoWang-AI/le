using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class OrganizationRegulatoryProgramModuleMap : EntityTypeConfiguration<OrganizationRegulatoryProgramModule>
    {
        #region constructors and destructor

        public OrganizationRegulatoryProgramModuleMap()
        {
            ToTable(tableName:"tOrganizationRegulatoryProgramModule");

            HasKey(x => x.OrganizationRegulatoryProgramModuleId);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.OrganizationRegulatoryProgramModules)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.ModuleId).IsRequired();
            HasRequired(a => a.Module)
                .WithMany(b => b.OrganizationRegulatoryProgramModules)
                .HasForeignKey(c => c.ModuleId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}