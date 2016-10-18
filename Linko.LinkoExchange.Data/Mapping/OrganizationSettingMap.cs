using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class OrganizationSettingMap : EntityTypeConfiguration<OrganizationSetting>
    {
        public OrganizationSettingMap()
        {
            ToTable("tOrganizationSetting");

            HasKey(i => i.OrganizationSettingId);
            Property(i => i.OrganizationId);
            //Property(i => i.SettingTemplate);
            Property(i => i.Value);
        }
    }
}
