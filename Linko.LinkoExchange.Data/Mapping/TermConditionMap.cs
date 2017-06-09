using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class TermConditionMap : EntityTypeConfiguration<TermCondition>
    {
        public TermConditionMap()
        {
            ToTable("tTermCondition");

            Property(x => x.TermConditionId).IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.Content).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}