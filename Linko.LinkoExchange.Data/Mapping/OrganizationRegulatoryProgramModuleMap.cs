using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public partial class OrganizationRegulatoryProgramModuleMap : EntityTypeConfiguration<OrganizationRegulatoryProgramModule>
    {
        public OrganizationRegulatoryProgramModuleMap()
        {
            ToTable("tOrganizationRegulatoryProgramModule");

            HasKey(x => x.OrganizationRegulatoryProgramModuleId);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.OrganizationRegulatoryProgramModules)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);

            Property(x => x.ModuleId).IsRequired();
            HasRequired(a => a.Module)
                .WithMany(b => b.OrganizationRegulatoryProgramModules)
                .HasForeignKey(c => c.ModuleId)
                .WillCascadeOnDelete(false);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}