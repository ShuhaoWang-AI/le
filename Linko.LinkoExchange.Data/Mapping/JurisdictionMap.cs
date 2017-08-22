using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class JurisdictionMap : EntityTypeConfiguration<Jurisdiction>
    {
        #region constructors and destructor

        public JurisdictionMap()
        {
            ToTable(tableName:"tJurisdiction");

            HasKey(x => x.JurisdictionId);

            Property(x => x.CountryId).IsRequired();

            Property(x => x.StateId).IsRequired();

            Property(x => x.Code).IsRequired().HasMaxLength(value:2).IsFixedLength();

            Property(x => x.Name).IsRequired().HasMaxLength(value:100);

            Property(x => x.ParentId).IsOptional();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}