using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public partial class CopyOfRecordMap : EntityTypeConfiguration<CopyOfRecord>
    {
        public CopyOfRecordMap()
        {
            ToTable("tCopyOfRecord");

            HasKey(x => x.CopyOfRecordId);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.CopyOfRecords)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.ReportPackage)
                .WithMany(b => b.CopyOfRecords)
                .HasForeignKey(c => c.ReportPackageId)
                .WillCascadeOnDelete(false);

            Property(x => x.Signature).IsRequired().HasMaxLength(350);

            Property(x => x.SignatureAlgorithm).IsRequired().HasMaxLength(10);

            Property(x => x.Hash).IsRequired().HasMaxLength(100);

            Property(x => x.HashAlgorithm).IsRequired().HasMaxLength(10);

            Property(x => x.Data).IsRequired();

            HasRequired(a => a.CopyOfRecordCertificate)
                .WithMany()
                .HasForeignKey(c => c.CopyOfRecordCertificateId)
                .WillCascadeOnDelete(false);
        }
    }
}