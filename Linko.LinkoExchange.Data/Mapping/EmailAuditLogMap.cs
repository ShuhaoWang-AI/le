using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class EmailAuditLogMap : EntityTypeConfiguration<EmailAuditLog>
    {
        #region constructors and destructor

        public EmailAuditLogMap()
        {
            ToTable(tableName:"tEmailAuditLog");

            HasKey(x => x.EmailAuditLogId);

            HasRequired(a => a.AuditLogTemplate)
                .WithMany()
                .HasForeignKey(c => c.AuditLogTemplateId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.SenderRegulatoryProgramId).IsOptional();

            Property(x => x.SenderOrganizationId).IsOptional();

            Property(x => x.SenderRegulatorOrganizationId).IsOptional();

            Property(x => x.SenderUserProfileId).IsOptional();

            Property(x => x.SenderUserName).IsOptional().HasMaxLength(value:256);

            Property(x => x.SenderFirstName).IsOptional().HasMaxLength(value:50);

            Property(x => x.SenderLastName).IsOptional().HasMaxLength(value:50);

            Property(x => x.SenderEmailAddress).IsRequired().HasMaxLength(value:256);

            Property(x => x.RecipientRegulatoryProgramId).IsOptional();

            Property(x => x.RecipientOrganizationId).IsOptional();

            Property(x => x.RecipientRegulatorOrganizationId).IsOptional();

            Property(x => x.RecipientUserProfileId).IsOptional();

            Property(x => x.RecipientUserName).IsOptional().HasMaxLength(value:256);

            Property(x => x.RecipientFirstName).IsOptional().HasMaxLength(value:50);

            Property(x => x.RecipientLastName).IsOptional().HasMaxLength(value:50);

            Property(x => x.RecipientEmailAddress).IsRequired().HasMaxLength(value:256);

            Property(x => x.Subject).IsRequired().HasMaxLength(value:500);

            Property(x => x.Body).IsRequired();

            Property(x => x.Token).IsOptional().HasMaxLength(value:128);

            Property(x => x.SentDateTimeUtc).IsRequired();
        }

        #endregion
    }
}