using Linko.LinkoExchange.Web.ViewModels.Shared;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Linko.LinkoExchange.Web.ViewModels.User
{
    public class UserProfileViewModel
    {

        [ScaffoldColumn(false)]
        [Editable(false)]
        [Display(Name = "UserProfileId")]
        public int UserProfileId
        {
            get; set;
        }

        // For permission role 
        [Editable(false)]
        [Required]
        [Display(Name ="Role")]
        public string Role
        {
            get;set;
        }

        [Editable(false)]
        [Display(Name ="Signatory")]
        [Required]
        public bool HasSigntory
        {
            get;set;
        }

        [Editable(false)]
        [Display(Name = "HasSigntoryText")]
        [Required]
        public string HasSigntoryText
        {
            get { return HasSigntory ? "Yes" : "No"; }
        }

        [Display(Name = "TitleRole")]
        public int TitleRole
        {
            get; set;
        }
 
        [Required]
        [Display(Name = "First Name")]
        public string FirstName
        {
            get; set;
        }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName
        {
            get; set;
        }

        [Required]
        [Display(Name = "Organization Name")]
        public string BusinessName
        {
            get; set;
        }

        [Required]
        [Editable(false)]
        public int OrganizationId
        {
            get; set;
        }

        [Display(Name = "Address Line1")]
        public string AddressLine1
        {
            get; set;
        }

        [Display(Name = "Address Line2")]
        public string AddressLine2
        {
            get; set;
        }

        [Required]
        [Display(Name = "City")]
        public string CityName
        {
            get; set;
        }

        [Required]
        [Display(Name = "State")]
        public int JurisdictionId
        {
            get; set;
        } 

        [Required]
        [Display(Name = "ZipCode")]
        public string ZipCode
        {
            get; set;
        }         
        
        [Required]
        [Display(Name = "Phone")]
        [Phone]
        public string PhoneNumber
        {
            get; set;
        }
         
        [Display(Name = "Ext")]
        public int? PhoneExt
        {
            get; set;
        } 

        [Display(Name = "Fax")]
        public string Fax
        {
            get; set;
        }
 
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email
        {
            get; set;
        }
        
        [Required]
        [Display(Name = "User Name")] 
        public string UserName
        {
            get; set;
        }

        [Required]
        [Display(Name = "Password")]
        [MinLength(8)]
        public string Password
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
         
        public List<JurisdictionViewModel> StateList
        {
            get;set;
        }
    }
}