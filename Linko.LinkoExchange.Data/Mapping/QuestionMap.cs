using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class QuestionMap : EntityTypeConfiguration<Question>
    {
        #region constructors and destructor

        public QuestionMap()
        {
            ToTable(tableName:"tQuestion");

            HasKey(x => x.QuestionId);

            Property(x => x.Content).IsRequired().HasMaxLength(value:500);

            HasRequired(a => a.QuestionType)
                .WithMany(b => b.Questions)
                .HasForeignKey(c => c.QuestionTypeId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.IsActive).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}