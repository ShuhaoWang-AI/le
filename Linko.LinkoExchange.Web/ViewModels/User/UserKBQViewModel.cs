﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;

namespace Linko.LinkoExchange.Web.ViewModels.User
{
    [Validator(validatorType:typeof(KbqValidator))]
    public class UserKBQViewModel
    {
        #region public properties

        [ScaffoldColumn(scaffold:false)]
        [Editable(allowEdit:false)]
        [Display(Name = "UserProfileId")]
        public int UserProfileId { get; set; }

        public int UserQuestionAnserId_KBQ1 { get; set; }

        public int UserQuestionAnserId_KBQ2 { get; set; }

        public int UserQuestionAnserId_KBQ3 { get; set; }

        public int UserQuestionAnserId_KBQ4 { get; set; }

        public int UserQuestionAnserId_KBQ5 { get; set; }

        [Display(Name = "Question 1")]
        public int KBQ1 { get; set; }

        [Display(Name = "Answer 1")]
        public string KBQAnswer1 { get; set; }

        [Display(Name = "Question 2")]
        public int KBQ2 { get; set; }

        [Display(Name = "Answer 2")]
        public string KBQAnswer2 { get; set; }

        [Display(Name = "Question 3")]
        public int KBQ3 { get; set; }

        [Display(Name = "Answer 3")]
        public string KBQAnswer3 { get; set; }

        [Display(Name = "Question 4")]
        public int KBQ4 { get; set; }

        [Display(Name = "Answer 4")]
        public string KBQAnswer4 { get; set; }

        [Display(Name = "Question 5")]
        public int KBQ5 { get; set; }

        [Display(Name = "Answer 5")]
        public string KBQAnswer5 { get; set; }

        public List<QuestionViewModel> QuestionPool { get; set; }

        #endregion
    }

    public class KbqValidator : AbstractValidator<UserKBQViewModel>
    {
        #region constructors and destructor

        public KbqValidator()
        {
            RuleFor(x => x.KBQ1)
                .NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.")
                .NotEqual(a => a.KBQ2).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQ3).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQ4).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQ5).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.");

            RuleFor(x => x.KBQ2)
                .NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.")
                .NotEqual(a => a.KBQ1).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQ3).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQ4).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQ5).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.");

            RuleFor(x => x.KBQ3)
                .NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.")
                .NotEqual(a => a.KBQ1).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQ2).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQ4).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQ5).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.");

            RuleFor(x => x.KBQ4)
                .NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.")
                .NotEqual(a => a.KBQ1).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQ2).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQ3).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQ5).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.");

            RuleFor(x => x.KBQ5)
                .NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.")
                .NotEqual(a => a.KBQ1).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQ2).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQ3).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQ4).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.");

            RuleFor(x => x.KBQAnswer1)
                .NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.")
                .NotEqual(a => a.KBQAnswer2).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQAnswer3).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQAnswer4).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQAnswer5).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.");

            RuleFor(x => x.KBQAnswer2)
                .NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.")
                .NotEqual(a => a.KBQAnswer1).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQAnswer3).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQAnswer4).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQAnswer5).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.");

            RuleFor(x => x.KBQAnswer3)
                .NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.")
                .NotEqual(a => a.KBQAnswer1).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQAnswer2).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQAnswer4).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQAnswer5).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.");

            RuleFor(x => x.KBQAnswer4)
                .NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.")
                .NotEqual(a => a.KBQAnswer1).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQAnswer2).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQAnswer3).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQAnswer5).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.");

            RuleFor(x => x.KBQAnswer5)
                .NotEmpty().WithMessage(errorMessage:"{PropertyName} is required.")
                .NotEqual(a => a.KBQAnswer1).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQAnswer2).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQAnswer3).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.")
                .NotEqual(a => a.KBQAnswer4).WithMessage(errorMessage:"{PropertyName} cannot be duplicated with others.");
        }

        #endregion
    }
}