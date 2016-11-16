using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class OrganizationSettingMap : EntityTypeConfiguration<OrganizationSetting>
    {
        public OrganizationSettingMap()
        {
            ToTable("tOrganizationSetting");

            HasKey(x => x.OrganizationSettingId);

            HasRequired(a => a.Organization)
                .WithMany(b => b.OrganizationSettings)
                .HasForeignKey(c => c.OrganizationId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.SettingTemplate)
                .WithMany(b => b.OrganizationSettings)
                .HasForeignKey(c => c.SettingTemplateId)
                .WillCascadeOnDelete(false);

            Property(x => x.Value).IsRequired().HasMaxLength(500);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}