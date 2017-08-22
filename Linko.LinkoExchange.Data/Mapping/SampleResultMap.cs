using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class SampleResultMap : EntityTypeConfiguration<SampleResult>
    {
        #region constructors and destructor

        public SampleResultMap()
        {
            ToTable(tableName:"tSampleResult");

            HasKey(x => x.SampleResultId);

            HasRequired(a => a.Sample)
                .WithMany(b => b.SampleResults)
                .HasForeignKey(c => c.SampleId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.ParameterId).IsRequired();

            Property(x => x.ParameterName).IsRequired().HasMaxLength(value:256);

            Property(x => x.Qualifier).IsOptional().HasMaxLength(value:2);

            Property(x => x.EnteredValue).IsOptional().HasMaxLength(value:50);

            Property(x => x.Value).IsOptional();

            Property(x => x.UnitId).IsRequired();

            Property(x => x.UnitName).IsRequired().HasMaxLength(value:50);

            Property(x => x.EnteredMethodDetectionLimit).IsOptional().HasMaxLength(value:50);

            Property(x => x.MethodDetectionLimit).IsOptional();

            Property(x => x.AnalysisMethod).IsOptional().HasMaxLength(value:50);

            Property(x => x.AnalysisDateTimeUtc).IsOptional();

            Property(x => x.IsApprovedEPAMethod).IsRequired();

            Property(x => x.IsMassLoadingCalculationRequired).IsRequired();

            Property(x => x.IsCalculated).IsRequired();

            HasOptional(a => a.LimitType)
                .WithMany()
                .HasForeignKey(c => c.LimitTypeId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.LimitBasis)
                .WithMany()
                .HasForeignKey(c => c.LimitBasisId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}