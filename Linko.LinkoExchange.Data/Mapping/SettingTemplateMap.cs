using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class SettingTemplateMap : EntityTypeConfiguration<SettingTemplate>
    {
        public SettingTemplateMap()
        {
            ToTable("tSettingTemplate");

            HasKey(x => x.SettingTemplateId);

            Property(x => x.Name).IsRequired().HasMaxLength(100);

            Property(x => x.Description).IsOptional().HasMaxLength(500);

            Property(x => x.DefaultValue).IsRequired().HasMaxLength(500);

            HasRequired(a => a.OrganizationType)
                .WithMany()
                .HasForeignKey(c => c.OrganizationTypeId)
                .WillCascadeOnDelete(false);

            HasOptional(a => a.RegulatoryProgram)
                .WithMany()
                .HasForeignKey(c => c.RegulatoryProgramId)
                .WillCascadeOnDelete(false);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}