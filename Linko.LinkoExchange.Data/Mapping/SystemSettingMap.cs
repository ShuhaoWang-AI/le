using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class SystemSettingMap : EntityTypeConfiguration<SystemSetting>
    {
        public SystemSettingMap()
        {
            ToTable("tSystemSetting");

            HasKey(x => x.SystemSettingId);

            Property(x => x.Name).IsRequired().HasMaxLength(100);

            Property(x => x.Value).IsRequired().HasMaxLength(500);

            Property(x => x.Description).IsOptional().HasMaxLength(500);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}