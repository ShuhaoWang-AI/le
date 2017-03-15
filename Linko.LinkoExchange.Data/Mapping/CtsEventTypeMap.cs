using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class CtsEventTypeMap : EntityTypeConfiguration<CtsEventType>
    {
        public CtsEventTypeMap()
        {
            ToTable("tCtsEventType");

            HasKey(x => x.CtsEventTypeId);
            Property(x => x.Name);
            Property(x => x.Description);
            Property(x => x.CtsEventCategoryName);
            Property(x => x.IsRemoved);
            Property(x => x.OrganizationRegulatoryProgramId);
            Property(x => x.CreationDateTimeUtc);
            Property(x => x.CreationDateTimeUtc);
            Property(x => x.LastModificationDateTimeUtc);
            Property(x => x.LastModifierUserId);
        }
    }
}