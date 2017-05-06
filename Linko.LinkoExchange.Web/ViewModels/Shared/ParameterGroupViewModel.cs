﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentValidation;
using FluentValidation.Attributes;

namespace Linko.LinkoExchange.Web.ViewModels.Shared
{
    [Validator(validatorType:typeof(ParameterGroupViewModelValidator))]
    public class ParameterGroupViewModel
    {
        [ScaffoldColumn(scaffold:false)]
        public int? Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        [DataType(dataType:DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Status")]
        public string Status => IsActive ? "Active" : "Inactive";

        [Display(Name = "Created/Modified Date")]
        public DateTime LastModificationDateTimeLocal { get; set; }

        [Display(Name = "Last Modified By")]
        public string LastModifierUserName { get; set; }

        public ICollection<ParameterViewModel> Parameters { get; set; }
        public List<ParameterViewModel> AllParameters { private get; set; }

        public List<ParameterViewModel> AvailableParameters
        {
            get
            {
                if (AllParameters != null && Parameters != null)
                {
                    return AllParameters.Where(a => Parameters.All(b => a.Id != b.Id)).ToList();
                }
                else
                {
                    return new List<ParameterViewModel>();
                }
            }
        }

        public string ParameterIds { get; set; }
    }

    public class ParameterGroupViewModelValidator:AbstractValidator<ParameterGroupViewModel>
    {
        public ParameterGroupViewModelValidator()
        {
            //Name
            RuleFor(x => x.Name).NotEmpty().WithMessage(errorMessage:"Parameter Group Name is required.");

            //Parameters
            RuleFor(x => x).Must(x => x.Parameters != null && x.Parameters.Count > 0).WithName(overridePropertyName:".").WithMessage(errorMessage:"At least 1 parameter must be added to the group");
        }
    }
}