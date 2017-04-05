using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public partial class SampleResultMap : EntityTypeConfiguration<SampleResult>
    {
        public SampleResultMap()
        {
            ToTable("tSampleResult");

            HasKey(x => x.SampleResultId);

            HasRequired(a => a.Sample)
                 .WithMany(b => b.SampleResults)
                 .HasForeignKey(c => c.SampleId)
                 .WillCascadeOnDelete(false);

            Property(x => x.ParameterId).IsRequired();

            Property(x => x.ParameterName).IsRequired();

            Property(x => x.Qualifier).IsRequired();

            Property(x => x.Value).IsRequired();

            Property(x => x.DecimalPlaces).IsRequired();

            Property(x => x.UnitId).IsRequired();

            Property(x => x.UnitName).IsRequired();

            Property(x => x.MethodDetectionLimit).IsOptional();

            Property(x => x.AnalysisMethod).IsOptional();

            Property(x => x.AnalysisDateTimeUtc).IsOptional();

            Property(x => x.IsApprovedEPAMethod).IsRequired();

            Property(x => x.IsMassLoadingCalculationRequired).IsRequired();

            Property(x => x.IsFlowForMassLoadingCalculation).IsRequired();

            Property(x => x.IsCalculated).IsRequired();

            HasOptional(a => a.LimitType)
                .WithMany(b => b.SampleResults)
                .HasForeignKey(c => c.LimitTypeId)
                .WillCascadeOnDelete(false);

            HasOptional(a => a.LimitBasis)
               .WithMany(b => b.SampleResults)
               .HasForeignKey(c => c.LimitBasisId)
               .WillCascadeOnDelete(false);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}