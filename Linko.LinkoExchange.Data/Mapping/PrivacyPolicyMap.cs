using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class PrivacyPolicyMap : EntityTypeConfiguration<PrivacyPolicy>
    {
        #region constructors and destructor

        public PrivacyPolicyMap()
        {
            ToTable(tableName:"tPrivacyPolicy");

            Property(x => x.PrivacyPolicyId).IsRequired().HasDatabaseGeneratedOption(databaseGeneratedOption:DatabaseGeneratedOption.Identity);

            Property(x => x.Content).IsRequired();

            Property(x => x.EffectiveDateTimeUtc).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}