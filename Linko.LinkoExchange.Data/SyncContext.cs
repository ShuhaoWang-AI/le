using System.Data.Entity;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data
{
    public class SyncContext : DbContext
    {
        #region constructors and destructor

        public SyncContext(string nameOrConnectionString)
            : base(nameOrConnectionString:nameOrConnectionString)
        {
            // Disable database initialization when the DB is not found
            Database.SetInitializer<SyncContext>(strategy:null);
        }

        #endregion

        #region utilities

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LEReportPackageParsedData>()
                        .ToTable(tableName:"tLEReportPackageParsedData")
                        .HasKey(x => x.LEReportPackageParsedDataID);

            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LinkoVersionNo).IsRequired().HasMaxLength(value:50);
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LinkoClientID).IsRequired();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEPermitNo).IsRequired().HasMaxLength(value:50);
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageID).IsRequired();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageName).HasMaxLength(value:100).IsRequired();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageCTSEventTypeName).IsRequired().HasMaxLength(value:50);
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageCTSEventCategoryName).IsRequired().HasMaxLength(value:50);
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPeriodStartDate).IsRequired();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPeriodEndDate).IsRequired();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageElements).IsOptional();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageSubmissionComments).IsOptional().HasMaxLength(value:500);
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageSampleComplianceSummary).IsOptional();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageURL).HasMaxLength(value:500).IsOptional();

            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageSubmissionDate).IsRequired();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageSubmissionBy).IsRequired().HasMaxLength(value:101);

            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageReviewedDate).IsOptional();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageReviewedBy).IsOptional().HasMaxLength(value:101);
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageReviewedByComments).HasMaxLength(value:500).IsOptional();

            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageLastSentDate).IsOptional();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageLastSentToCTSBy).HasMaxLength(value:101).IsOptional();

            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageElementTypeID).IsOptional();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageElementCTSEventTypeName).IsOptional().HasMaxLength(value:50);
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageElementCTSEventCategoryName).IsOptional().HasMaxLength(value:50);

            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageElementCategoryName).HasMaxLength(value:100).IsOptional();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageElementTypeName).HasMaxLength(value:100).IsOptional();

            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageIsRepudiated).IsRequired();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageRepudiatedDate).IsOptional();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageRepudiatedBy).HasMaxLength(value:101).IsOptional();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageRepudiatedByReason).IsOptional().HasMaxLength(value:100);
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageRepudiatedByComments).IsOptional().HasMaxLength(value:500);

            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageRepudiationReviewedByDate).IsOptional();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageRepudiationReviewedBy).IsOptional().HasMaxLength(value:101);
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageRepudiatedReviewedByComments).IsOptional().HasMaxLength(value:500);

            modelBuilder.Entity<LESampleResultParsedData>()
                        .ToTable(tableName:"tLESampleResultParsedData")
                        .HasKey(x => x.LESampleResultParsedDataID);

            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LinkoVersionNo).IsRequired().HasMaxLength(value:50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LinkoClientID).IsRequired();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabPermitMPID).HasMaxLength(value:250).IsRequired();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabSampleName).HasMaxLength(value:50).IsRequired();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabDateSampled).IsRequired();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabStartDateTimeSampled).IsOptional();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabStopDateTimeSampled).IsOptional();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabSampleID).IsOptional().HasMaxLength(value:50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabSampleEventType).IsOptional().HasMaxLength(value:50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabCollectMethod).IsOptional().HasMaxLength(value:50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabSampler).IsOptional().HasMaxLength(value:254);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabAnalysisDate).IsOptional();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabAnalysisMethod).IsOptional().HasMaxLength(value:50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabParamName).IsRequired().HasMaxLength(value:254);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabResult).IsRequired().HasMaxLength(value:50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabNumResult).IsOptional();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabResultFlag).IsOptional().HasMaxLength(value:50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabResultUnits).IsRequired().HasMaxLength(value:50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabRepLimit).IsOptional().HasMaxLength(value:50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabMDL).IsOptional().HasMaxLength(value:50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.IsLabQCResult).IsOptional();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabReportedDate).IsOptional();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabSampleComments).IsOptional().HasMaxLength(value:1000);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabResultComments).IsOptional().HasMaxLength(value:1000);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabStatus).IsOptional().HasMaxLength(value:50);

            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabSampleMatchID).IsOptional().HasMaxLength(value:50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.IsResultRepudiated).IsOptional();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LEIsValidEPAMethod).IsOptional();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LEReportPackageID).IsRequired();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LESampleID).IsRequired();
        }

        #endregion

        #region DbSets

        public DbSet<LEReportPackageParsedData> LEReportPackageParsedDatas { get; set; }
        public DbSet<LESampleResultParsedData> LESampleResultParsedDatas { get; set; }

        #endregion
    }
}