using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ParameterGroupParameterMap : EntityTypeConfiguration<ParameterGroupParameter>
    {
        #region constructors and destructor

        public ParameterGroupParameterMap()
        {
            ToTable(tableName:"tParameterGroupParameter");

            HasKey(x => x.ParameterGroupParameterId);

            HasRequired(a => a.ParameterGroup)
                .WithMany(b => b.ParameterGroupParameters)
                .HasForeignKey(c => c.ParameterGroupId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.Parameter)
                .WithMany()
                .HasForeignKey(c => c.ParameterId)
                .WillCascadeOnDelete(value:false);
        }

        #endregion
    }
}