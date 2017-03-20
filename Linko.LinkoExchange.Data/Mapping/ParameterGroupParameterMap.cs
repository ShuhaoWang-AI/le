using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ParameterGroupParameterMap : EntityTypeConfiguration<ParameterGroupParameter>
    {
        public ParameterGroupParameterMap()
        {
            ToTable("tParameterGroupParameter");

            HasKey(x => x.ParameterGroupParameterId);

            HasRequired(a => a.ParameterGroup)
                .WithMany(b => b.ParameterGroupParameters)
                .HasForeignKey(c => c.ParameterGroupId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.Parameter)
                .WithMany()
                .HasForeignKey(c => c.ParameterId)
                .WillCascadeOnDelete(false);
        }
    }
}