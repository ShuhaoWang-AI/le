using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public partial class ReportPackageMap : EntityTypeConfiguration<ReportPackage>
    {
        public ReportPackageMap()
        {
            ToTable("tReportPackage");

            HasKey(x => x.ReportPackageId);

            Property(x => x.Name).IsRequired().HasMaxLength(100);

            Property(x => x.Description).IsOptional().HasMaxLength(500);

            Property(x => x.PeriodStartDateTimeUtc).IsRequired();

            Property(x => x.PeriodEndDateTimeUtc).IsRequired();

            Property(x => x.CtsEventTypeId).IsOptional();

            Property(x => x.CtsEventTypeName).IsOptional().HasMaxLength(100);

            Property(x => x.CtsEventCategoryName).IsOptional().HasMaxLength(100);

            Property(x => x.Comments).IsOptional().HasMaxLength(500);

            Property(x => x.IsSubmissionBySignatoryRequired).IsRequired();


            Property(x => x.ReportPackageTemplateId).IsRequired();

            HasRequired(a => a.ReportStatus)
                .WithMany()
                .HasForeignKey(c => c.ReportStatusId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.ReportPackages)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);

            Property(x => x.OrganizationName).IsRequired().HasMaxLength(254);

            Property(x => x.OrganizationAddressLine1).IsRequired().HasMaxLength(100);

            Property(x => x.OrganizationAddressLine2).IsRequired().HasMaxLength(100);

            Property(x => x.OrganizationCityName).IsRequired().HasMaxLength(100);

            Property(x => x.OrganizationJurisdictionName).IsRequired().HasMaxLength(100);

            Property(x => x.OrganizationZipCode).IsRequired().HasMaxLength(50);

            Property(x => x.RecipientOrganizationName).IsRequired().HasMaxLength(254);

            Property(x => x.RecipientOrganizationAddressLine1).IsRequired().HasMaxLength(100);

            Property(x => x.RecipientOrganizationAddressLine2).IsRequired().HasMaxLength(100);

            Property(x => x.RecipientOrganizationCityName).IsRequired().HasMaxLength(100);

            Property(x => x.RecipientOrganizationJurisdictionName).IsRequired().HasMaxLength(100);

            Property(x => x.RecipientOrganizationZipCode).IsRequired().HasMaxLength(50);


            Property(x => x.SubmissionDateTimeUtc).IsOptional();

            Property(x => x.SubmitterUserId).IsOptional();

            Property(x => x.SubmitterFirstName).IsOptional().HasMaxLength(50);

            Property(x => x.SubmitterLastName).IsOptional().HasMaxLength(50);

            Property(x => x.SubmitterTitleRole).IsOptional().HasMaxLength(250);

            Property(x => x.SubmitterIPAddress).IsOptional().HasMaxLength(50);

            Property(x => x.SubmitterUserName).IsOptional().HasMaxLength(256);


            Property(x => x.SubmissionReviewDateTimeUtc).IsOptional();

            Property(x => x.SubmissionReviewerUserId).IsOptional();

            Property(x => x.SubmissionReviewerFirstName).IsOptional().HasMaxLength(50);

            Property(x => x.SubmissionReviewerLastName).IsOptional().HasMaxLength(50);

            Property(x => x.SubmissionReviewerTitleRole).IsOptional().HasMaxLength(250);

            Property(x => x.SubmissionReviewComments).IsOptional().HasMaxLength(500);


            Property(x => x.RepudiationDateTimeUtc).IsOptional();

            Property(x => x.RepudiatorUserId).IsOptional();

            Property(x => x.RepudiatorFirstName).IsOptional().HasMaxLength(50);

            Property(x => x.RepudiatorLastName).IsOptional().HasMaxLength(50);

            Property(x => x.RepudiatorTitleRole).IsOptional().HasMaxLength(250);

            Property(x => x.RepudiationReasonId).IsOptional();

            Property(x => x.RepudiationReasonName).IsOptional().HasMaxLength(100);

            Property(x => x.RepudiationComments).IsOptional().HasMaxLength(500);


            Property(x => x.RepudiationReviewDateTimeUtc).IsOptional();

            Property(x => x.RepudiationReviewerUserId).IsOptional();

            Property(x => x.RepudiationReviewerFirstName).IsOptional().HasMaxLength(50);

            Property(x => x.RepudiationReviewerLastName).IsOptional().HasMaxLength(50);

            Property(x => x.RepudiationReviewerTitleRole).IsOptional().HasMaxLength(250);

            Property(x => x.RepudiationReviewComments).IsOptional().HasMaxLength(500);


            Property(x => x.LastSentDateTimeUtc).IsOptional();

            Property(x => x.LastSenderUserId).IsOptional();

            Property(x => x.LastSenderFirstName).IsOptional().HasMaxLength(50);

            Property(x => x.LastSenderLastName).IsOptional().HasMaxLength(50);


            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();

            HasOptional(a => a.CopyOfRecord)
                .WithRequired(b => b.ReportPackage);
        }
    }
}