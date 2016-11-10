using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class SettingTemplateMap : EntityTypeConfiguration<SettingTemplate>
    {
        public SettingTemplateMap()
        {
            ToTable("tSettingTemplate");

            HasKey(i => i.SettingTemplateId);
            Property(i => i.Name);
            Property(i => i.Description);
            Property(i => i.DefaultValue);
            Property(i => i.OrganizationTypeId);
            HasRequired(i => i.OrganizationType)
               .WithMany()
               .HasForeignKey(i => i.OrganizationTypeId);
        }
    }
}
