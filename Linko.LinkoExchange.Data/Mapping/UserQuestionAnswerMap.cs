using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class UserQuestionAnswerMap : EntityTypeConfiguration<UserQuestionAnswer>
    {
        public UserQuestionAnswerMap()
        {
            ToTable("tUserQuestionAnswer");

            HasKey(x => x.UserQuestionAnswerId);

            Property(x => x.Content).IsRequired();

            HasRequired(a => a.Question)
                .WithMany()
                .HasForeignKey(c => c.QuestionId)
                .WillCascadeOnDelete(false);

            Property(x => x.UserProfileId).IsRequired();
            //HasRequired(a => a.UserProfile)
            //    .WithMany()
            //    .HasForeignKey(c => c.UserProfileId)
            //    .WillCascadeOnDelete(false);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();
        }
    }
}