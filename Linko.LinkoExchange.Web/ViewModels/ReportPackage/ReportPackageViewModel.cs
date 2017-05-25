using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Web.ViewModels.Shared;

namespace Linko.LinkoExchange.Web.ViewModels.ReportPackage
{
    [Validator(validatorType:typeof(ReportPackageViewModelValidator))]
    public class ReportPackageViewModel
    {
        [ScaffoldColumn(scaffold:false)]
        public int? Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Period Start Date")]
        public DateTime PeriodStartDateTimeLocal { get; set; }

        [Display(Name = "Period End Date")]
        public DateTime PeriodEndDateTimeLocal { get; set; }

        [Display(Name = "Report Period")]
        public string ReportPeriod => $"{PeriodStartDateTimeLocal:MM/dd/yyyy} - {PeriodEndDateTimeLocal:MM/dd/yyyy}";

        [Display(Name = "CTS Event Type")]
        public int? CtsEventTypeId { get; set; }

        [Display(Name = "CTS Event Type")]
        public string CtsEventTypeName { get; set; }

        [Display(Name = "Comments")]
        [DataType(dataType:DataType.MultilineText)]
        public string Comments { get; set; }

        [Display(Name = "Is Submission By Signatory Required")]
        public bool IsSubmissionBySignatoryRequired { get; set; }

        public bool IsCurrentUserSignatory { get; set; } // need to check current user has signatory right or not before submit report

        [ScaffoldColumn(scaffold:false)]
        public int QuestionAnswerId { get; set; }

        public string Question { get; set; }

        public string Answer { get; set; }

        [DataType(dataType:DataType.Password)]
        [Display(Name = "Enter Your Password")]
        public string Password { get; set; }

        [Display(Name = "Status")]
        public ReportStatusName Status { get; set; }

        [ScaffoldColumn(scaffold:false)]
        public int OrganizationRegulatoryProgramId { get; set; }

        [Display(Name = "Industry No")]
        public string OrganizationReferenceNumber { get; set; }

        [Display(Name = "Industry")]
        public string OrganizationName { get; internal set; }

        [Display(Name = "Submitted")]
        public DateTime? SubmissionDateTimeLocal { get; set; }

        public string SubmitterFirstName { get; set; }
        public string SubmitterLastName { get; set; }

        [Display(Name = "Submitted By")]
        public string Submitter => $"{SubmitterFirstName} {SubmitterLastName}";

        [Display(Name = "Title")]
        public string SubmitterTitleRole { get; set; }

        [Display(Name = "Last Reviewed")]
        public DateTime? SubmissionReviewDateTimeLocal { get; set; }

        public string SubmissionReviewerFirstName { get; set; }
        public string SubmissionReviewerLastName { get; set; }

        [Display(Name = "Last Reviewed By")]
        public string SubmissionReviewer => $"{SubmissionReviewerFirstName} {SubmissionReviewerLastName}";

        [Display(Name = "Comments")]
        public string SubmissionReviewComments { get; set; }

        [Display(Name = "Repudiated")]
        public DateTime? RepudiationDateTimeLocal { get; set; }

        public string RepudiatorFirstName { get; set; }
        public string RepudiatorLastName { get; set; }

        [Display(Name = "Repudiated By")]
        public string Repudiator => $"{RepudiatorFirstName} {RepudiatorLastName}";

        [Display(Name = "Title")]
        public string RepudiatorTitleRole { get; set; }

        [Display(Name = "Reason")]
        public int? RepudiationReasonId { get; set; }

        [Display(Name = "Reason")]
        public string RepudiationReasonName { get; set; }

        [Display(Name = "Comments")]
        public string RepudiationComments { get; set; }

        [Display(Name = "Last Reviewed")]
        public DateTime? RepudiationReviewDateTimeLocal { get; set; }

        public string RepudiationReviewerFirstName { get; set; }
        public string RepudiationReviewerLastName { get; set; }

        [Display(Name = "Last Reviewed By")]
        public string RepudiationReviewer => $"{RepudiationReviewerFirstName} {RepudiationReviewerLastName}";

        [Display(Name = "Comments")]
        public string RepudiationReviewComments { get; set; }

        [Display(Name = "Last Sent to CTS")]
        public DateTime? LastSentDateTimeLocal { get; set; }

        public string LastSenderFirstName { get; set; }
        public string LastSenderLastName { get; set; }

        [Display(Name = "Last Sent to CTS By")]
        public string LastSender => $"{LastSenderFirstName} {LastSenderLastName}";

        [Display(Name = "Created/Modified Date")]
        public DateTime LastModificationDateTimeLocal { get; set; }

        [Display(Name = "Last Modified By")]
        public string LastModifierUserName { get; set; }

        [Display(Name = "Samples And Results")]
        public List<ReportElementTypeViewModel> SamplesAndResultsTypes { get; set; }

        [Display(Name = "Attachments")]
        public List<ReportElementTypeViewModel> AttachmentTypes { get; set; }

        [Display(Name = "Certifications")]
        public List<ReportElementTypeViewModel> CertificationTypes { get; set; }

        [Display(Name = "Categories")]
        public List<ReportElementCategoryName> ReportPackageTemplateElementCategories { get; set; }
    }

    public class ReportPackageViewModelValidator:AbstractValidator<ReportPackageViewModel> {}
}