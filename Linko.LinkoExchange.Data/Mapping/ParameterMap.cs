using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public partial class ParameterMap : EntityTypeConfiguration<Parameter>
    {
        public ParameterMap()
        {
            ToTable("tParameter");

            HasKey(x => x.ParameterId);

            Property(x => x.Name).IsRequired().HasMaxLength(254);

            Property(x => x.Description).IsOptional().HasMaxLength(500);

            HasOptional(a => a.DefaultUnit)
                .WithMany()
                .HasForeignKey(c => c.DefaultUnitId)
                .WillCascadeOnDelete(false);

            Property(x => x.TrcFactor).IsOptional();

            Property(x => x.IsFlowForMassLoadingCalculation).IsRequired();

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.Parameters)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);

            Property(x => x.IsRemoved).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}