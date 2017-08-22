using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class SettingTemplateMap : EntityTypeConfiguration<SettingTemplate>
    {
        #region constructors and destructor

        public SettingTemplateMap()
        {
            ToTable(tableName:"tSettingTemplate");

            HasKey(x => x.SettingTemplateId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:100);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            Property(x => x.DefaultValue).IsRequired().HasMaxLength(value:500);

            HasRequired(a => a.OrganizationType)
                .WithMany()
                .HasForeignKey(c => c.OrganizationTypeId)
                .WillCascadeOnDelete(value:false);

            HasOptional(a => a.RegulatoryProgram)
                .WithMany()
                .HasForeignKey(c => c.RegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}