using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Data.Mapping
{

    public class OrganizationMap : EntityTypeConfiguration<Organization>
    {
        public OrganizationMap()
        {
            this.ToTable("tOrganizationRegulatoryProgram");
            this.HasKey(o => o.OrgRegProgId);
            this.Property(o => o.OrgRegProgId).HasColumnName("OrganizationRegulatoryProgramId").IsRequired();
            this.Property(o => o.RegProgId).HasColumnName("RegulatoryProgramId").IsRequired();
            this.Property(o => o.OrganizationId).HasColumnName("OrganizationId").IsRequired();
            this.Property(o => o.RegulatorOrgId).HasColumnName("RegulatorOrganizationId").IsOptional();
            //this.Property(o => o.??).IsRequired().HasMaxLength(2).IsFixedLength();

            Map(m =>
            {
                m.Properties(e => new
                {
                    e.OrgRegProgId,
                    e.RegProgId,
                    e.OrganizationId,
                    e.RegulatorOrgId
                });
                m.ToTable("tOrganizationRegulatoryProgram");
            });

            Map(m =>
            {
                m.Properties(e => new
                {
                    e.OrganizationId,
                    e.Name,
                    e.AddressLine1
                });
                m.ToTable("tOrganization");
            });
        }
    }
}
