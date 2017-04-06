using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public partial class CopyOfRecordCertificateMap : EntityTypeConfiguration<CopyOfRecordCertificate>
    {
        public CopyOfRecordCertificateMap()
        {
            ToTable("tCopyOfRecordCertificate");

            HasKey(x => x.CopyOfRecordCertificateId);

            Property(x => x.PhysicalPath).IsRequired().HasMaxLength(256);

            Property(x => x.FileName).IsRequired().HasMaxLength(256);

            Property(x => x.Password).IsRequired().HasMaxLength(50);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}