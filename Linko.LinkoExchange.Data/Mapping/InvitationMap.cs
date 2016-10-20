using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    class InvitationMap : EntityTypeConfiguration<Invitation>
    {
        public InvitationMap()
        {
            ToTable("tInvitation");

            HasKey(i => i.InvitationId);
            Property(i => i.InvitationId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(i => i.FirstName);
            Property(i => i.LastName);
            Property(i => i.EmailAddress).IsRequired();
            Property(i => i.InvitationDateTimeUtc).IsRequired();
            Property(i => i.SenderOrganizationRegulatoryProgramId).IsRequired();
            Property(i => i.RecipientOrganizationRegulatoryProgramId).IsRequired(); 
        }
    }
}