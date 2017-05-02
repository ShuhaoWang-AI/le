using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{
    //TODO: to add more, 
    // Here is just for Cor usage
    public class ReportPackageDto
    {
        public int ReportPackageId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset PeriodStartDateTime { get; set; }
        public DateTimeOffset PeriodEndDateTime { get; set; }
        public DateTime PeriodStartDateTimeLocal { get; internal set; }
        public DateTime PeriodEndDateTimeLocal { get; internal set; }
        public string Comments { get; set; }
        public bool IsSubmissionBySignatoryRequired { get; internal set; }
        public int ReportStatusId { get; internal set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public string OrganizationName { get; internal set; }
        public string OrganizationAddressLine1 { get; internal set; }
        public string OrganizationAddressLine2 { get; internal set; }
        public string OrganizationCityName { get; internal set; }
        public string OrganizationJurisdictionName { get; internal set; }
        public string OrganizationZipCode { get; internal set; }

        public DateTimeOffset? SubmissionDateTimeOffset { get; set; }
        public DateTime? SubmissionDateTimeLocal { get; set; }
        public OrganizationRegulatoryProgramDto OrganizationRegulatoryProgramDto { get; set; }
        public string RecipientOrganizationName { get; internal set; }
        public string RecipientOrganizationAddressLine1 { get; internal set; }
        public string RecipientOrganizationAddressLine2 { get; internal set; }
        public string RecipientOrganizationCityName { get; internal set; }
        public string RecipientOrganizationJurisdictionName { get; internal set; }
        public string RecipientOrganizationZipCode { get; internal set; }
        public string SubmissionReviewerUserId { get; set; }
        public int SubmitterUserId { get; set; }
        public string SubmitterFirstName { get; set; }
        public string SubmitterLastName { get; set; }
        public string SubmitterTitleRole { get; set; }
        public string SubmitterIPAddress { get; set; }
        public string SubmitterUserName { get; set; }

        public DateTime? RepudiationDateTimeLocal { get; set; }
        public int? RepudiatorUserId { get; set; }
        public string RepudiatorFirstName { get; set; }
        public string RepudiatorLastName { get; set; }
        public string RepudiatorTitleRole { get; set; }
        public int? RepudiationReasonId { get; set; }
        public string RepudiationReasonName { get; set; }
        public string RepudiationComments { get; set; }

        public DateTime? RepudiationReviewDateTimeLocal { get; set; }
        public int? RepudiationReviewerUserId { get; set; }
        public string RepudiationReviewerFirstName { get; set; }
        public string RepudiationReviewerLastName { get; set; }
        public string RepudiationReviewerTitleRole { get; set; }
        public string RepudiationReviewComments { get; set; }

        public string PermitNumber { get; set; }
        public DateTime CreationDateTimeLocal { get; internal set; }
        public List<ReportFileDto> AssociatedFiles { get; set; }
        public List<ReportSampleDto> AssociatedSamples { get; set; }
        public List<ReportPackageElementTypeDto> CertificationDtos { get; set; }

    }
}