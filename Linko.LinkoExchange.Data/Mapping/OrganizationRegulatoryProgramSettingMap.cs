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

            HasKey(i => i.OrganizationRegulatoryProgramSettingId);
            //Property(i => i.SettingTemplate);
            Property(i => i.Value);
        }
    }
}
