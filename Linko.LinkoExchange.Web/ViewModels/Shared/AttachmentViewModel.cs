using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;

namespace Linko.LinkoExchange.Web.ViewModels.Shared
{
    [Validator(validatorType:typeof(AttachmentViewModelValidator))]
    public class AttachmentViewModel
    {
        [ScaffoldColumn(scaffold:false)]
        public int? Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Original File Name")]
        public string OriginalFileName { get; set; }

        [Display(Name = "Description")]
        [DataType(dataType:DataType.MultilineText)]
        public string Description { get; set; }

        public string MediaType { get; set; }

        [Display(Name = "Attachment Type")]
        public int ReportElementTypeId { get; set; }

        [Display(Name = "Attachment Type")]
        public string ReportElementTypeName { get; set; }

        public List<SelectListItem> AvailableReportElementTypes { get; set; }

        [Display(Name = "Upload Date")]
        public DateTime UploadDateTimeLocal { get; set; }

        [Display(Name = "Upload By")]
        public string UploaderUserFullName { get; set; }

        [Display(Name = "Created/Modified Date")]
        public DateTime LastModificationDateTimeLocal { get; set; }

        [Display(Name = "Last Modified By")]
        public string LastModifierUserName { get; set; }

        public bool UsedByReports { get; set; }

        [Display(Name = "Status")]
        public string Status => UsedByReports ? "Reported" : "Draft";

        public string AllowedFileExtensions { get; set; }
    }

    public class AttachmentViewModelValidator:AbstractValidator<AttachmentViewModel>
    {
        public AttachmentViewModelValidator()
        {
            //ReportElementTypeId
            RuleFor(x => x.ReportElementTypeId).GreaterThan(valueToCompare:0).WithMessage(errorMessage:"{PropertyName} is required.");
        }
    }
}