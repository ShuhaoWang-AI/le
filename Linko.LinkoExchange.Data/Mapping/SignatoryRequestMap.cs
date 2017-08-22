using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class SignatoryRequestMap : EntityTypeConfiguration<SignatoryRequest>
    {
        #region constructors and destructor

        public SignatoryRequestMap()
        {
            ToTable(tableName:"tSignatoryRequest");

            HasKey(x => x.SignatoryRequestId);

            Property(x => x.RequestDateTimeUtc).IsRequired();

            Property(x => x.GrantDenyDateTimeUtc).IsRequired();

            Property(x => x.RevokeDateTimeUtc).IsRequired();

            HasRequired(a => a.OrganizationRegulatoryProgramUser)
                .WithMany(b => b.SignatoryRequests)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramUserId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.SignatoryRequestStatus)
                .WithMany(b => b.SignatoryRequests)
                .HasForeignKey(c => c.SignatoryRequestStatusId)
                .WillCascadeOnDelete(value:false);
        }

        #endregion
    }
}