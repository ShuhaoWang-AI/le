using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class RegulatoryProgramMap : EntityTypeConfiguration<RegulatoryProgram>
    {
        public RegulatoryProgramMap()
        {
            ToTable("tRegulatoryProgram");

            HasKey(i => i.RegulatoryProgramId);
            Property(i => i.Name);
            Property(i => i.Description);
        }
    }
}
