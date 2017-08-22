using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class TermConditionMap : EntityTypeConfiguration<TermCondition>
    {
        #region constructors and destructor

        public TermConditionMap()
        {
            ToTable(tableName:"tTermCondition");

            Property(x => x.TermConditionId).IsRequired().HasDatabaseGeneratedOption(databaseGeneratedOption:DatabaseGeneratedOption.Identity);

            Property(x => x.Content).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}