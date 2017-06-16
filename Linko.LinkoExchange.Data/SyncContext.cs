using System;
using System.Data.Entity;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data
{
    public class SyncContext : DbContext
    {
        #region constructor

        public SyncContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {

        }

        #endregion


        #region DbSets

        public DbSet<LEReportPackageParsedData> LEReportPackageParsedDatas { get; set; }
        public DbSet<LESampleResultParsedData> LESampleResultParsedDatas { get; set; }

        #endregion


        #region utilities

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LEReportPackageParsedData>()
                        .ToTable("tLEReportPackageParsedData")
                        .HasKey(x => x.LEReportPackageParsedDataID);

            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LinkoVersionNo).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LinkoClientID).IsRequired();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEPermitNo).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageID).IsRequired();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageName).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageCTSEventTypeName).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageCTSEventCategoryName).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPeriodStartDate).IsRequired();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPeriodEndDate).IsRequired();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageElements).IsOptional();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageSubmissionComments).IsOptional().HasMaxLength(500);
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageSampleComplianceSummary).IsOptional();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageURL).HasMaxLength(500).IsOptional();

            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageSubmissionDate).IsRequired();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageSubmissionBy).IsRequired().HasMaxLength(101);

            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageReviewedDate).IsOptional();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageReviewedBy).IsOptional().HasMaxLength(101);
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageReviewedByComments).HasMaxLength(500).IsOptional();

            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageLastSentDate).IsOptional();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageLastSentToCTSBy).HasMaxLength(101).IsOptional();

            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageElementTypeID).IsOptional();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageElementCTSEventTypeName).IsOptional().HasMaxLength(50);
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageElementCTSEventCategoryName).IsOptional().HasMaxLength(50);

            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageElementCategoryName).HasMaxLength(100).IsOptional();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageElementTypeName).HasMaxLength(100).IsOptional();

            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageIsRepudiated).IsRequired();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageRepudiatedDate).IsOptional();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageRepudiatedBy).HasMaxLength(101).IsOptional();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageRepudiatedByReason).IsOptional().HasMaxLength(100);
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageRepudiatedByComments).IsOptional().HasMaxLength(500);

            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageRepudiationReviewedByDate).IsOptional();
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageRepudiationReviewedBy).IsOptional().HasMaxLength(101);
            modelBuilder.Entity<LEReportPackageParsedData>().Property(x => x.LEReportPackageRepudiatedReviewedByComments).IsOptional().HasMaxLength(500);



            modelBuilder.Entity<LESampleResultParsedData>()
                        .ToTable("tLESampleResultParsedData")
                        .HasKey(x => x.LESampleResultParsedDataID);

            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LinkoVersionNo).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LinkoClientID).IsRequired();
            //modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LinkoLabID).IsRequired();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabPermitMPID).HasMaxLength(250).IsRequired();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabSampleName).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabDateSampled).IsRequired();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabStartDateTimeSampled).IsOptional();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabStopDateTimeSampled).IsOptional();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabSampleID).IsOptional().HasMaxLength(50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabSampleEventType).IsOptional().HasMaxLength(50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabCollectMethod).IsOptional().HasMaxLength(50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabSampler).IsOptional().HasMaxLength(254);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabAnalysisDate).IsOptional();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabAnalysisMethod).IsOptional().HasMaxLength(50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabParamName).IsRequired().HasMaxLength(254);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabResult).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabNumResult).IsOptional();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabResultFlag).IsOptional().HasMaxLength(50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabResultUnits).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabRepLimit).IsOptional().HasMaxLength(50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabMDL).IsOptional().HasMaxLength(50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.IsLabQCResult).IsOptional();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabReportedDate).IsOptional();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabSampleComments).IsOptional().HasMaxLength(1000);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabResultComments).IsOptional().HasMaxLength(1000);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabStatus).IsOptional().HasMaxLength(50);

            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LabSampleMatchID).IsOptional().HasMaxLength(50);
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.IsResultRepudiated).IsOptional();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LEIsValidEPAMethod).IsOptional();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LEReportPackageID).IsRequired();
            modelBuilder.Entity<LESampleResultParsedData>().Property(x => x.LESampleID).IsRequired();
        }

        #endregion
    }
}