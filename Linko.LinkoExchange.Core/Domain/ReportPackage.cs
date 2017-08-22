using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a report package.
    ///     It contains a snapshot of most objects contained in this report and hence, they are denormalized.
    /// </summary>
    public class ReportPackage
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int ReportPackageId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTimeOffset PeriodStartDateTimeUtc { get; set; }

        public DateTimeOffset PeriodEndDateTimeUtc { get; set; }

        public int? CtsEventTypeId { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string CtsEventTypeName { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string CtsEventCategoryName { get; set; }

        public string Comments { get; set; }

        public bool IsSubmissionBySignatoryRequired { get; set; }

        public int ReportPackageTemplateId { get; set; }

        public int ReportStatusId { get; set; }
        public virtual ReportStatus ReportStatus { get; set; }

        /// <summary>
        ///     Typical value: Industry Regulatory Program id.
        /// </summary>
        public int OrganizationRegulatoryProgramId { get; set; }

        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string OrganizationReferenceNumber { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string OrganizationName { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string OrganizationAddressLine1 { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string OrganizationAddressLine2 { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string OrganizationCityName { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string OrganizationJurisdictionName { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string OrganizationZipCode { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string RecipientOrganizationName { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string RecipientOrganizationAddressLine1 { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string RecipientOrganizationAddressLine2 { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string RecipientOrganizationCityName { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string RecipientOrganizationJurisdictionName { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string RecipientOrganizationZipCode { get; set; }

        public DateTimeOffset? SubmissionDateTimeUtc { get; set; }

        public int? SubmitterUserId { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string SubmitterFirstName { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string SubmitterLastName { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string SubmitterTitleRole { get; set; }

        public string SubmitterIPAddress { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string SubmitterUserName { get; set; }

        public DateTimeOffset? SubmissionReviewDateTimeUtc { get; set; }

        public int? SubmissionReviewerUserId { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string SubmissionReviewerFirstName { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string SubmissionReviewerLastName { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string SubmissionReviewerTitleRole { get; set; }

        public string SubmissionReviewComments { get; set; }

        public DateTimeOffset? RepudiationDateTimeUtc { get; set; }

        public int? RepudiatorUserId { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string RepudiatorFirstName { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string RepudiatorLastName { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string RepudiatorTitleRole { get; set; }

        public int? RepudiationReasonId { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string RepudiationReasonName { get; set; }

        public string RepudiationComments { get; set; }

        public DateTimeOffset? RepudiationReviewDateTimeUtc { get; set; }

        public int? RepudiationReviewerUserId { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string RepudiationReviewerFirstName { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string RepudiationReviewerLastName { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string RepudiationReviewerTitleRole { get; set; }

        public string RepudiationReviewComments { get; set; }

        public DateTimeOffset? LastSentDateTimeUtc { get; set; }

        public int? LastSenderUserId { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string LastSenderFirstName { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string LastSenderLastName { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        // Reverse navigation
        public virtual ICollection<ReportPackageElementCategory> ReportPackageElementCategories { get; set; }

        public virtual CopyOfRecord CopyOfRecord { get; set; }

        #endregion
    }
}