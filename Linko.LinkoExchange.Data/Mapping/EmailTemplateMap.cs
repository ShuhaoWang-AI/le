using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    class EmailTemplateMap : EntityTypeConfiguration<EmailTemplate>
    {
        public EmailTemplateMap()
        {
            ToTable("tEmailTemplates");

            HasKey(i => i.Id);

            Property(i => i.EmailType).HasMaxLength(50);
            Property(i => i.Template).IsMaxLength();
        }
    }
}
