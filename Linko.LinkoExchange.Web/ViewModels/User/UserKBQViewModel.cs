﻿using Linko.LinkoExchange.Web.ViewModels.Shared;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Linko.LinkoExchange.Web.ViewModels.User
{
    public class UserKBQViewModel
    {

        [ScaffoldColumn(false)]
        [Editable(false)]
        [Display(Name = "UserProfileId")]
        public int UserProfileId
        {
            get; set;
        }

        public int UserQuestionAnserId_KBQ1
        {
            get; set;
        }

        public int UserQuestionAnserId_KBQ2
        {
            get; set;
        }

        public int UserQuestionAnserId_KBQ3
        {
            get; set;
        }

        public int UserQuestionAnserId_KBQ4
        {
            get; set;
        }

        public int UserQuestionAnserId_KBQ5
        {
            get; set;
        }
        
        [Required]
        [Display(Name = "Question 1")] 
        public int KBQ1
        {
            get; set;
        }

        [Required]
        [Display(Name = "Answer 1")] 
        public string KBQAnswer1
        {
            get; set;
        }

        [Required]
        [Display(Name = "Question 2")] 
        public int KBQ2
        {
            get; set;
        }

        [Required]
        [Display(Name = "Answer 2")] 
        public string KBQAnswer2
        {
            get; set;
        }

        [Required]
        [Display(Name = "Question 3")]
        public int KBQ3
        {
            get; set;
        }

        [Required]
        [Display(Name = "Answer 3")] 
        public string KBQAnswer3
        {
            get; set;
        }

        [Required]
        [Display(Name = "Question 4")]
        public int KBQ4
        {
            get; set;
        }

        [Required]
        [Display(Name = "Answer 4")] 
        public string KBQAnswer4
        {
            get; set;
        }

        [Required]
        [Display(Name = "Question 5")]
        public int KBQ5
        {
            get; set;
        }

        [Required]
        [Display(Name = "Answer 5")] 
        public string KBQAnswer5
        {
            get; set;
        } 

        public List<QuestionViewModel> QuestionPool
        {
            get;set;
        }
    }
}