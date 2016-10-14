using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    class EmailAuditLogMap : EntityTypeConfiguration<EmailAuditLog>
    {
        public EmailAuditLogMap()
        {
            ToTable("tEmailAuditLog");

            HasKey(i => i.EmailAuditLogId);

            Property(i => i.AuditLogTemplateId);
            Property(i => i.SenderRegulatorId);

            Property(i => i.SenderRegulateeId);
            Property(i => i.SenderRegulatoryProgramId);

            Property(i => i.SenderUserName);
            Property(i => i.SenderFirstName);
            Property(i => i.SenderLastName);
            Property(i => i.SenderEmailAddress); 

            Property(i => i.RecipientRegulatorId);
            Property(i => i.RecipientRegulateeId);
            Property(i => i.RecipientRegulatoryProgramId);

            Property(i => i.RecipientUserName); 
            Property(i => i.RecipientFirstName);
            Property(i => i.RecipientLastName);
            Property(i => i.RecipientEmailAddress);

            Property(i => i.Subject);
            Property(i => i.Body);
            Property(i => i.SentDateTimeUtc);
        }
    }
}