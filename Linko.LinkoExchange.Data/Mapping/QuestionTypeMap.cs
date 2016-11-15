using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class QuestionTypeMap : EntityTypeConfiguration<QuestionType>
    {
        public QuestionTypeMap()
        {
            ToTable("tQuestionType");
            HasKey(i => i.QuestionTypeId);
            Property(i => i.Name);
        }
    }
}
