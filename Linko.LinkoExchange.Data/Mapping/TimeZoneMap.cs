using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Data.Mapping
{ 

    public class TimeZoneMap : EntityTypeConfiguration<Core.Domain.TimeZone>
    {
        public TimeZoneMap()
        {
            ToTable("tTimeZone");

            HasKey(i => i.TimeZoneId);
            Property(i => i.Abbreviation);
            Property(i => i.Name);
        }
    }
}
