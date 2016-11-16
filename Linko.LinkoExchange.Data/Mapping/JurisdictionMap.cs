using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class JurisdictionMap : EntityTypeConfiguration<Jurisdiction>
    {
        public JurisdictionMap()
        {
            ToTable("tJurisdiction");

            HasKey(x => x.JurisdictionId);

            Property(x => x.CountryId).IsRequired();

            Property(x => x.StateId).IsRequired();

            Property(x => x.Code).IsRequired().HasMaxLength(2).IsFixedLength();

            Property(x => x.Name).IsRequired().HasMaxLength(100);

            Property(x => x.ParentId).IsOptional();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}