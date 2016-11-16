using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class QuestionMap : EntityTypeConfiguration<Question>
    {
        public QuestionMap()
        {
            ToTable("tQuestion");

            HasKey(x => x.QuestionId);

            Property(x => x.Content).IsRequired().HasMaxLength(500);

            HasRequired(a => a.QuestionType)
                .WithMany(b => b.Questions)
                .HasForeignKey(c => c.QuestionTypeId)
                .WillCascadeOnDelete(false);

            Property(x => x.IsActive).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}