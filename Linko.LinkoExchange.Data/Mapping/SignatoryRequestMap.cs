using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class SignatoryRequestMap : EntityTypeConfiguration<SignatoryRequest>
    {
        public SignatoryRequestMap()
        {
            ToTable("tSignatoryRequest");

            HasKey(x => x.SignatoryRequestId);

            Property(x => x.RequestDateTimeUtc).IsRequired();

            Property(x => x.GrantDenyDateTimeUtc).IsRequired();

            Property(x => x.RevokeDateTimeUtc).IsRequired();

            HasRequired(a => a.OrganizationRegulatoryProgramUser)
                .WithMany(b => b.SignatoryRequests)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramUserId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.SignatoryRequestStatus)
                .WithMany(b => b.SignatoryRequests)
                .HasForeignKey(c => c.SignatoryRequestStatusId)
                .WillCascadeOnDelete(false);
        }
    }
}