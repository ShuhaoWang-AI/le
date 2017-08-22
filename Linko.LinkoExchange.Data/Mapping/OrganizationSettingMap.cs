using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class OrganizationSettingMap : EntityTypeConfiguration<OrganizationSetting>
    {
        #region constructors and destructor

        public OrganizationSettingMap()
        {
            ToTable(tableName:"tOrganizationSetting");

            HasKey(x => x.OrganizationSettingId);

            HasRequired(a => a.Organization)
                .WithMany(b => b.OrganizationSettings)
                .HasForeignKey(c => c.OrganizationId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.SettingTemplate)
                .WithMany(b => b.OrganizationSettings)
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