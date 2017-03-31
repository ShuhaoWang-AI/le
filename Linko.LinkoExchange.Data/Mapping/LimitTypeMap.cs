using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class LimitTypeMap : EntityTypeConfiguration<LimitType>
    {
        public LimitTypeMap()
        {
            ToTable("tLimitType");

            HasKey(x => x.LimitTypeId);

            Property(x => x.Name).IsRequired().HasMaxLength(254);

            Property(x => x.Description).IsOptional().HasMaxLength(500);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}
