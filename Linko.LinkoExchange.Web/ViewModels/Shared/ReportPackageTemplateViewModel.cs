﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Web.ViewModels.Authority;

namespace Linko.LinkoExchange.Web.ViewModels.Shared
{
    [Validator(validatorType:typeof(ReportPackageTemplateViewModelValidator))]
    public class ReportPackageTemplateViewModel
    {
        [ScaffoldColumn(scaffold:false)]
        public int? Id { get; set; }

        [Display(Name = "Template Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        [DataType(dataType:DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "Effective Date")]
        public DateTime EffectiveDateTimeLocal { get; set; }

        [Display(Name = "Retirement Date")]
        public DateTime? RetirementDateTimeLocal { get; set; }

        [Display(Name = "Is Submission By Signatory Required")]
        public bool IsSubmissionBySignatoryRequired => true;

        public int CtsEventTypeId { get; set; }

        [Display(Name = "CTS Event Type")]
        public string CtsEventTypeName { get; set; }

        public IList<SelectListItem> AvailableCtsEventTypes { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Status")]
        public string Status => IsActive ? "Active" : "Inactive";

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
        
        [Display(Name = "Industries")]
        public List<IndustryViewModel> ReportPackageTemplateAssignments { get; set; }
        
        [Display(Name = "Categories")]
        public List<ReportElementCategoryName> ReportPackageTemplateElementCategories { get; set; }

        [Display(Name = "Show Sample Results")]
        public bool ShowSampleResults => true;
    }

    public class ReportPackageTemplateViewModelValidator:AbstractValidator<ReportPackageTemplateViewModel>
    {
        public ReportPackageTemplateViewModelValidator()
        {
            //Name
            RuleFor(x => x.Name).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");

            //Parameters
            //RuleFor(x => x).Must(x => (x.Parameters != null) && (x.Parameters.Count > 0)).WithName(".").WithMessage("At least 1 parameter must be added to the group");
        }
    }
}