using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class PrivacyPolicyMap : EntityTypeConfiguration<PrivacyPolicy>
    {
        public PrivacyPolicyMap()
        {
            ToTable("tPrivacyPolicy");

            Property(x => x.PrivacyPolicyId).IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.Content).IsRequired();

            Property(x => x.EffectiveDateTimeUtc).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}