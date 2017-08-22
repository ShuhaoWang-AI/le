using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class UserQuestionAnswerMap : EntityTypeConfiguration<UserQuestionAnswer>
    {
        #region constructors and destructor

        public UserQuestionAnswerMap()
        {
            ToTable(tableName:"tUserQuestionAnswer");

            HasKey(x => x.UserQuestionAnswerId);

            Property(x => x.Content).IsRequired();

            HasRequired(a => a.Question)
                .WithMany()
                .HasForeignKey(c => c.QuestionId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.UserProfileId).IsRequired();

            //HasRequired(a => a.UserProfile)
            //    .WithMany()
            //    .HasForeignKey(c => c.UserProfileId)
            //    .WillCascadeOnDelete(false);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();
        }

        #endregion
    }
}