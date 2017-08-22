using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportPackageMap : EntityTypeConfiguration<ReportPackage>
    {
        #region constructors and destructor

        public ReportPackageMap()
        {
            ToTable(tableName:"tReportPackage");

            HasKey(x => x.ReportPackageId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:100);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            Property(x => x.PeriodStartDateTimeUtc).IsRequired();

            Property(x => x.PeriodEndDateTimeUtc).IsRequired();

            Property(x => x.CtsEventTypeId).IsOptional();

            Property(x => x.CtsEventTypeName).IsOptional().HasMaxLength(value:50);

            Property(x => x.CtsEventCategoryName).IsOptional().HasMaxLength(value:50);

            Property(x => x.Comments).IsOptional().HasMaxLength(value:500);

            Property(x => x.IsSubmissionBySignatoryRequired).IsRequired();

            Property(x => x.ReportPackageTemplateId).IsRequired();

            HasRequired(a => a.ReportStatus)
                .WithMany()
                .HasForeignKey(c => c.ReportStatusId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.ReportPackages)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.OrganizationReferenceNumber).IsOptional().HasMaxLength(value:50);

            Property(x => x.OrganizationName).IsRequired().HasMaxLength(value:254);

            Property(x => x.OrganizationAddressLine1).IsOptional().HasMaxLength(value:100);

            Property(x => x.OrganizationAddressLine2).IsOptional().HasMaxLength(value:100);

            Property(x => x.OrganizationCityName).IsOptional().HasMaxLength(value:100);

            Property(x => x.OrganizationJurisdictionName).IsOptional().HasMaxLength(value:100);

            Property(x => x.OrganizationZipCode).IsOptional().HasMaxLength(value:50);

            Property(x => x.RecipientOrganizationName).IsRequired().HasMaxLength(value:254);

            Property(x => x.RecipientOrganizationAddressLine1).IsOptional().HasMaxLength(value:100);

            Property(x => x.RecipientOrganizationAddressLine2).IsOptional().HasMaxLength(value:100);

            Property(x => x.RecipientOrganizationCityName).IsOptional().HasMaxLength(value:100);

            Property(x => x.RecipientOrganizationJurisdictionName).IsOptional().HasMaxLength(value:100);

            Property(x => x.RecipientOrganizationZipCode).IsOptional().HasMaxLength(value:50);

            Property(x => x.SubmissionDateTimeUtc).IsOptional();

            Property(x => x.SubmitterUserId).IsOptional();

            Property(x => x.SubmitterFirstName).IsOptional().HasMaxLength(value:50);

            Property(x => x.SubmitterLastName).IsOptional().HasMaxLength(value:50);

            Property(x => x.SubmitterTitleRole).IsOptional().HasMaxLength(value:250);

            Property(x => x.SubmitterIPAddress).IsOptional().HasMaxLength(value:50);

            Property(x => x.SubmitterUserName).IsOptional().HasMaxLength(value:256);

            Property(x => x.SubmissionReviewDateTimeUtc).IsOptional();

            Property(x => x.SubmissionReviewerUserId).IsOptional();

            Property(x => x.SubmissionReviewerFirstName).IsOptional().HasMaxLength(value:50);

            Property(x => x.SubmissionReviewerLastName).IsOptional().HasMaxLength(value:50);

            Property(x => x.SubmissionReviewerTitleRole).IsOptional().HasMaxLength(value:250);

            Property(x => x.SubmissionReviewComments).IsOptional().HasMaxLength(value:500);

            Property(x => x.RepudiationDateTimeUtc).IsOptional();

            Property(x => x.RepudiatorUserId).IsOptional();

            Property(x => x.RepudiatorFirstName).IsOptional().HasMaxLength(value:50);

            Property(x => x.RepudiatorLastName).IsOptional().HasMaxLength(value:50);

            Property(x => x.RepudiatorTitleRole).IsOptional().HasMaxLength(value:250);

            Property(x => x.RepudiationReasonId).IsOptional();

            Property(x => x.RepudiationReasonName).IsOptional().HasMaxLength(value:100);

            Property(x => x.RepudiationComments).IsOptional().HasMaxLength(value:500);

            Property(x => x.RepudiationReviewDateTimeUtc).IsOptional();

            Property(x => x.RepudiationReviewerUserId).IsOptional();

            Property(x => x.RepudiationReviewerFirstName).IsOptional().HasMaxLength(value:50);

            Property(x => x.RepudiationReviewerLastName).IsOptional().HasMaxLength(value:50);

            Property(x => x.RepudiationReviewerTitleRole).IsOptional().HasMaxLength(value:250);

            Property(x => x.RepudiationReviewComments).IsOptional().HasMaxLength(value:500);

            Property(x => x.LastSentDateTimeUtc).IsOptional();

            Property(x => x.LastSenderUserId).IsOptional();

            Property(x => x.LastSenderFirstName).IsOptional().HasMaxLength(value:50);

            Property(x => x.LastSenderLastName).IsOptional().HasMaxLength(value:50);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();

            HasOptional(a => a.CopyOfRecord)
                .WithRequired(b => b.ReportPackage);
        }

        #endregion
    }
}