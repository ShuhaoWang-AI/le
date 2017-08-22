using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class CopyOfRecordCertificateMap : EntityTypeConfiguration<CopyOfRecordCertificate>
    {
        #region constructors and destructor

        public CopyOfRecordCertificateMap()
        {
            ToTable(tableName:"tCopyOfRecordCertificate");

            HasKey(x => x.CopyOfRecordCertificateId);

            Property(x => x.PhysicalPath).IsRequired().HasMaxLength(value:256);

            Property(x => x.FileName).IsRequired().HasMaxLength(value:256);

            Property(x => x.Password).IsRequired().HasMaxLength(value:50);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}