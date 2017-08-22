using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class CopyOfRecordMap : EntityTypeConfiguration<CopyOfRecord>
    {
        #region constructors and destructor

        public CopyOfRecordMap()
        {
            ToTable(tableName:"tCopyOfRecord");

            HasKey(x => x.ReportPackageId);

            Property(x => x.Signature).IsRequired().HasMaxLength(value:350);

            Property(x => x.SignatureAlgorithm).IsRequired().HasMaxLength(value:10);

            Property(x => x.Hash).IsRequired().HasMaxLength(value:100);

            Property(x => x.HashAlgorithm).IsRequired().HasMaxLength(value:10);

            Property(x => x.Data).IsRequired();

            HasRequired(a => a.CopyOfRecordCertificate)
                .WithMany()
                .HasForeignKey(c => c.CopyOfRecordCertificateId)
                .WillCascadeOnDelete(value:false);
        }

        #endregion
    }
}