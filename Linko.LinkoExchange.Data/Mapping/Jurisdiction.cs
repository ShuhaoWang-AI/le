using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Data.Mapping
{ 

    public class JurisdictionMap : EntityTypeConfiguration<Jurisdiction>
    {
        public JurisdictionMap()
        {
            ToTable("tJurisdiction");

            HasKey(i => i.JurisdictionId);
            Property(i => i.CountryId);
            Property(i => i.StateId);
            Property(i => i.Code);
            Property(i => i.Name);  
        }
    }
}
