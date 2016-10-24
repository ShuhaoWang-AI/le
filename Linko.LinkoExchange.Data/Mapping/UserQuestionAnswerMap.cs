using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class UserQuestionAnswerMap : EntityTypeConfiguration<UserQuestionAnswer>
    {
        public UserQuestionAnswerMap()
        {
            ToTable("tUserQuestionAnswer");
            HasKey(i => i.UserQuestionAnswerId);
            Property(i => i.Content);
            HasRequired(i => i.Question)
                //.WithOptional(o => o.)
                .WithMany()
                .HasForeignKey(i => i.QuestionId);

            Property(i => i.UserProfileId);
            Property(i => i.CreationDateTimeUtc);
            Property(i => i.LastModificationDateTimeUtc);
        }
    }
}
