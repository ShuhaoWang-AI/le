﻿using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class UnitDimensionMap : EntityTypeConfiguration<UnitDimension>
    {
        #region constructors and destructor

        public UnitDimensionMap()
        {
            ToTable(tableName:"tUnitDimension");

            HasKey(x => x.UnitDimensionId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:50);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}