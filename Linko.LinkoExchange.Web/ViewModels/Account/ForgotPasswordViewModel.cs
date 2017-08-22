﻿using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;
using Linko.LinkoExchange.Core.Resources;

namespace Linko.LinkoExchange.Web.ViewModels.Account
{
    [Validator(validatorType:typeof(ForgotPasswordValidator))]
    public class ForgotPasswordViewModel
    {
        #region public properties

        [Display(Name = "UserName", ResourceType = typeof(Label))]
        public string UserName { get; set; }

        #endregion
    }

    public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordViewModel>
    {
        #region constructors and destructor

        public ForgotPasswordValidator()
        {
            //UserName
            RuleFor(x => x.UserName).NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.");
            RuleFor(x => x.UserName.Length).LessThanOrEqualTo(valueToCompare:256).WithMessage(errorMessage:"{PropertyName} must be less than or equal to 256 characters long.");
        }

        #endregion
    }
}