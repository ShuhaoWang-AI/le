using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class CopyOfRecordCertificateMap : EntityTypeConfiguration<CopyOfRecordCertificate>
    {
        public CopyOfRecordCertificateMap()
        {
            ToTable("tCopyOfRecordCertificateMap");

            HasKey(t => t.CopyOfRecordCertificateId);
            Property(t => t.PhysicalPath).IsRequired();
            Property(t => t.FileName).IsRequired();
            Property(t => t.Password).IsRequired();

            Property(t => t.CreationDateTimeUtc).IsRequired();
            Property(t => t.LastModificationDateTimeUtc);
            Property(t => t.LastModifierUserId);
        }
    }
}