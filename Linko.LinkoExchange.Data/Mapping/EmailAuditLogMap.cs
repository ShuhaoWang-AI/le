using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class EmailAuditLogMap : EntityTypeConfiguration<EmailAuditLog>
    {
        public EmailAuditLogMap()
        {
            ToTable("tEmailAuditLog");

            HasKey(x => x.EmailAuditLogId);

            HasRequired(a => a.AuditLogTemplate)
                .WithMany()
                .HasForeignKey(c => c.AuditLogTemplateId)
                .WillCascadeOnDelete(false);

            Property(x => x.SenderRegulatoryProgramId).IsOptional();

            Property(x => x.SenderOrganizationId).IsOptional();

            Property(x => x.SenderRegulatorOrganizationId).IsOptional();

            Property(x => x.SenderUserProfileId).IsOptional();

            Property(x => x.SenderUserName).IsOptional().HasMaxLength(256);

            Property(x => x.SenderFirstName).IsOptional().HasMaxLength(50);

            Property(x => x.SenderLastName).IsOptional().HasMaxLength(50);

            Property(x => x.SenderEmailAddress).IsRequired().HasMaxLength(256);

            Property(x => x.RecipientRegulatoryProgramId).IsOptional();

            Property(x => x.RecipientOrganizationId).IsOptional();

            Property(x => x.RecipientRegulatorOrganizationId).IsOptional();

            Property(x => x.RecipientUserProfileId).IsOptional();

            Property(x => x.RecipientUserName).IsOptional().HasMaxLength(256);

            Property(x => x.RecipientFirstName).IsOptional().HasMaxLength(50);

            Property(x => x.RecipientLastName).IsOptional().HasMaxLength(50);

            Property(x => x.RecipientEmailAddress).IsRequired().HasMaxLength(256);

            Property(x => x.Subject).IsRequired().HasMaxLength(500);

            Property(x => x.Body).IsRequired();

            Property(x => x.Token).IsOptional().HasMaxLength(128);

            Property(x => x.SentDateTimeUtc).IsRequired();
        }
    }
}