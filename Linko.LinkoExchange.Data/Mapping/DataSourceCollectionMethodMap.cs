﻿using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class DataSourceCollectionMethodMap : EntityTypeConfiguration<DataSourceCollectionMethod>
    {
        #region constructors and destructor

        public DataSourceCollectionMethodMap()
        {
            ToTable(tableName:"tDataSourceCollectionMethod");

            HasKey(x => x.DataSourceCollectionMethodId);

            Property(x => x.DataSourceTerm).IsRequired().HasMaxLength(value:254);

            HasRequired(a => a.DataSource)
                .WithMany(b => b.DataSourceCollectionMethods)
                .HasForeignKey(c => c.DataSourceId)
                .WillCascadeOnDelete(value:false);

            HasRequired(x => x.CollectionMethod)
                .WithMany()
                .HasForeignKey(x => x.CollectionMethodId)
                .WillCascadeOnDelete(value:false);
        }

        #endregion
    }
}