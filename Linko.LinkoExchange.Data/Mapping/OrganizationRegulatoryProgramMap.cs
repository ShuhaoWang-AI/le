using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class OrganizationRegulatoryProgramMap : EntityTypeConfiguration<OrganizationRegulatoryProgram>
    {
        public OrganizationRegulatoryProgramMap()
        {
            ToTable("tOrganizationRegulatoryProgram");

            HasKey(i => i.OrganizationRegulatoryProgramId);
            Property(i => i.RegulatoryProgramId);
            Property(i => i.RegulatorOrganizationId);
            Property(i => i.IsEnabled);
        }
    }
}
