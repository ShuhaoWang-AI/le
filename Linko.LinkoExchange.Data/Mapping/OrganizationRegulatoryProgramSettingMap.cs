using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class OrganizationRegulatoryProgramSettingMap : EntityTypeConfiguration<OrganizationRegulatoryProgramSetting>
    {
        public OrganizationRegulatoryProgramSettingMap()
        {
            ToTable("tOrganizationRegulatoryProgramSetting");

            this.HasKey(i => i.OrganizationRegulatoryProgramSettingId);
            this.Property(i => i.Value);
            this.Property(i => i.SettingTemplateId);
            this.HasRequired(i => i.SettingTemplate)
                .WithMany()
                .HasForeignKey(i => i.SettingTemplateId);
            this.HasRequired(i => i.OrganizationRegulatoryProgram)
                .WithMany()
                .HasForeignKey(i => i.OrganizationRegulatoryProgramId);

        }
    }
}
