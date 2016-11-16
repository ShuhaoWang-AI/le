using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Web.ViewModels.User
{
    public class UserSQViewModel
    {

        [ScaffoldColumn(false)]
        [Editable(false)]
        [Display(Name = "UserProfileId")]
        public int UserProfileId
        {
            get; set;
        }
 
        public int UserQuestionAnserId_SQ1
        {
            get;set;
        }

        public int UserQuestionAnserId_SQ2
        {
            get; set;
        }
          
        [Required]
        [Display(Name = "Question 1")] 
        public int SecuritryQuestion1
        {
            get;set;
        }
 
        [Required]
        [Display(Name = "Answer 1")] 
        public string SecurityQuestionAnswer1
        {
            get;set;
        }

        [Required]
        [Display(Name = "Question 2")] 
        public int SecurityQuestion2
        {
            get; set;
        }

        [Required]
        [Display(Name = "Answer 2")] 
        public string SecurityQuestionAnswer2
        {
            get; set;
        } 

        public List<QuestionViewModel> QuestionPool
        {
            get;set;
        } 
    }
}