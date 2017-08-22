using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class OrganizationRegulatoryProgramSettingMap : EntityTypeConfiguration<OrganizationRegulatoryProgramSetting>
    {
        #region constructors and destructor

        public OrganizationRegulatoryProgramSettingMap()
        {
            ToTable(tableName:"tOrganizationRegulatoryProgramSetting");

            HasKey(x => x.OrganizationRegulatoryProgramSettingId);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.OrganizationRegulatoryProgramSettings)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.SettingTemplate)
                .WithMany(b => b.OrganizationRegulatoryProgramSettings)
                .HasForeignKey(c => c.SettingTemplateId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.Value).IsRequired().HasMaxLength(value:500);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}