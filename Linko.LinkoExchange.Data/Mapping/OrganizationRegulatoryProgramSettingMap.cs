using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class OrganizationRegulatoryProgramSettingMap : EntityTypeConfiguration<OrganizationRegulatoryProgramSetting>
    {
        public OrganizationRegulatoryProgramSettingMap()
        {
            ToTable("tOrganizationRegulatoryProgramSetting");

            HasKey(x => x.OrganizationRegulatoryProgramSettingId);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.OrganizationRegulatoryProgramSettings)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.SettingTemplate)
                .WithMany(b => b.OrganizationRegulatoryProgramSettings)
                .HasForeignKey(c => c.SettingTemplateId)
                .WillCascadeOnDelete(false);

            Property(x => x.Value).IsRequired().HasMaxLength(500);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}