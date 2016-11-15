using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class QuestionMap : EntityTypeConfiguration<Question>
    {
        public QuestionMap()
        {
            ToTable("tQuestion");
            HasKey(i => i.QuestionId);
            Property(i => i.Content);
            Property(i => i.QuestionTypeId);
            HasRequired(i => i.QuestionType)
                .WithMany()
                .HasForeignKey(i => i.QuestionTypeId);
            Property(i => i.IsActive);
            Property(i => i.CreationDateTimeUtc);
            Property(i => i.LastModificationDateTimeUtc);
        }
    }

}
