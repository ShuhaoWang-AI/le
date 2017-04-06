using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class CopyOfRecordMap : EntityTypeConfiguration<CopyOfRecord>
    {
        public CopyOfRecordMap()
        {
            ToTable("tCopyOfRecord");

            HasKey(t => t.CopyOfRecordId);
            Property(t => t.OrganizationRegulatoryProgramId).IsRequired();
            Property(t => t.ReportPackageId).IsRequired();
            Property(t => t.Signature).IsRequired();
            Property(t => t.SignatureAlgorithm).IsRequired();
            Property(t => t.Hash).IsRequired();
            Property(t => t.HashAlgorithm).IsRequired();
            Property(t => t.Data).IsRequired();

            HasRequired(t => t.CopyOfRecordCertificate)
                .WithMany()
                .HasForeignKey(i => i.CopyOfRecordCertificateId);
        }
    }
}