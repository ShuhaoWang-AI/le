using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class SignatoryRequestStatusMap : EntityTypeConfiguration<SignatoryRequestStatus>
    {
        #region constructors and destructor

        public SignatoryRequestStatusMap()
        {
            ToTable(tableName:"tSignatoryRequestStatus");

            HasKey(x => x.SignatoryRequestStatusId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:100);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}