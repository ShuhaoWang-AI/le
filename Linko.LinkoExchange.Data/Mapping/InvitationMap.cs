using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class InvitationMap : EntityTypeConfiguration<Invitation>
    {
        public InvitationMap()
        {
            ToTable("tInvitation");

            HasKey(x => x.InvitationId);

            Property(x => x.FirstName).IsRequired().HasMaxLength(50);

            Property(x => x.LastName).IsRequired().HasMaxLength(50);

            Property(x => x.EmailAddress).IsRequired().HasMaxLength(256);

            Property(x => x.InvitationDateTimeUtc).IsRequired();

            HasRequired(a => a.RecipientOrganizationRegulatoryProgram)
                .WithMany()
                .HasForeignKey(c => c.RecipientOrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.SenderOrganizationRegulatoryProgram)
                .WithMany()
                .HasForeignKey(c => c.SenderOrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);

            Property(x => x.IsResetInvitation).IsRequired();
        }
    }
}