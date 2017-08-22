using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class InvitationMap : EntityTypeConfiguration<Invitation>
    {
        #region constructors and destructor

        public InvitationMap()
        {
            ToTable(tableName:"tInvitation");

            HasKey(x => x.InvitationId);

            Property(x => x.FirstName).IsRequired().HasMaxLength(value:50);

            Property(x => x.LastName).IsRequired().HasMaxLength(value:50);

            Property(x => x.EmailAddress).IsRequired().HasMaxLength(value:256);

            Property(x => x.InvitationDateTimeUtc).IsRequired();

            HasRequired(a => a.RecipientOrganizationRegulatoryProgram)
                .WithMany()
                .HasForeignKey(c => c.RecipientOrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.SenderOrganizationRegulatoryProgram)
                .WithMany()
                .HasForeignKey(c => c.SenderOrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.IsResetInvitation).IsRequired();
        }

        #endregion
    }
}