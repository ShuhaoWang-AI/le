﻿using Linko.LinkoExchange.Web.ViewModels.Shared;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        [Display(Name ="Role")]
        public string Role
        {
            get;set;
        }

        [Editable(false)]
        [Display(Name ="Signatory")] 
        public bool HasSigntory
        {
            get;set;
        }

        [Editable(false)]
        [Display(Name = "Signatory")]
        [Required]
        public string HasSigntoryText
        {
            get { return HasSigntory ? "Yes" : "No"; }
        }

        [Display(Name = "Title/Role")]
        public string TitleRole
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

        [Display(Name = "Address Line 1")]
        public string AddressLine1
        {
            get; set;
        }

        [Display(Name = "Address Line 2")]
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
        [Display(Name = "Zip Code")]
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
         
        public List<JurisdictionViewModel> StateList
        {
            get;set;
        }
    }
}