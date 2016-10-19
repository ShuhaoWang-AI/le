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
            Property(i => i.OrganizationId);
            this.HasRequired(i => i.Organization)
                .WithMany()
                .HasForeignKey(i => i.OrganizationId);

            Property(i => i.RegulatorOrganizationId);
            this.HasRequired(i => i.RegulatorOrganization)
                .WithMany()
                .HasForeignKey(i => i.RegulatorOrganizationId);

            Property(i => i.IsEnabled);
        }
    }
}
