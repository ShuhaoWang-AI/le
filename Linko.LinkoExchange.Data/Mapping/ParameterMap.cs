using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ParameterMap : EntityTypeConfiguration<Parameter>
    {
        #region constructors and destructor

        public ParameterMap()
        {
            ToTable(tableName:"tParameter");

            HasKey(x => x.ParameterId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:254);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            HasRequired(a => a.DefaultUnit)
                .WithMany()
                .HasForeignKey(c => c.DefaultUnitId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.TrcFactor).IsOptional();

            Property(x => x.IsFlowForMassLoadingCalculation).IsRequired();

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.Parameters)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.IsRemoved).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}