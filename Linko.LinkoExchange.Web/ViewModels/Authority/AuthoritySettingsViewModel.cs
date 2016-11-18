using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Linko.LinkoExchange.Web.ViewModels.Authority
{
    public class AuthoritySettingsViewModel
    {
        #region Authority information

        [ScaffoldColumn(false)]
        [Display(Name = "ID")]
        public int ID
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Exchange Authority ID")]
        public int ExchangeAuthorityID
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Authority Name")]
        public string AuthorityName
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "NPDES")]
        public string Npdes
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Signer")]
        public string Signer
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Address line 1")]
        public string AddressLine1
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Address line 2")]
        public string AddressLine2
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "City")]
        public string CityName
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "State")]
        public string State
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Zip Code")]
        public string ZipCode
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Address")]
        public string Address
        {
            get
            {
                return string.Format(format: "{0} {1}, {2}, {3} {4}", args: new object[] { AddressLine1, AddressLine2, CityName, State, ZipCode });
            }
        }

        [Editable(false)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Ext")]
        public string PhoneExt
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Fax Number")]
        public string FaxNumber
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Website URL")]
        public string WebsiteUrl
        {
            get; set;
        }

        [Editable(false)]
        public bool HasPermissionForUpdate
        {
            get; set;
        }

        #endregion

        #region Authority settings

        [Display(Name = "Number of Allowed Failed Password Attempts")]
        public string FailedPasswordAttemptMaxCount
        {
            get; set;
        }

        [Editable(false)]
        public string FailedPasswordAttemptMaxCountDefault
        {
            get; set;
        }

        [Display(Name = "Number of Allowed Failed KBQ Attempts")]
        public string FailedKbqAttemptMaxCount
        {
            get; set;
        }

        [Editable(false)]
        public string FailedKbqAttemptMaxCountDefault
        {
            get; set;
        }

        [Display(Name = "Invitation Expires This Many Hours After Sending")]
        public string InvitationExpiredHours
        {
            get; set;
        }

        [Editable(false)]
        public string InvitationExpiredHoursDefault
        {
            get; set;
        }

        [Display(Name = "Number of Days Before Requiring a Password Change")]
        public string PasswordChangeRequiredDays
        {
            get; set;
        }

        [Editable(false)]
        public string PasswordChangeRequiredDaysDefault
        {
            get; set;
        }

        [Display(Name = "Number of Passwords Used in Password History")]
        public string PasswordHistoryMaxCount
        {
            get; set;
        }

        [Editable(false)]
        public string PasswordHistoryMaxCountDefault
        {
            get; set;
        }

        [Display(Name = "Time Zone")]
        public string TimeZone
        {
            get; set;
        }

        public IList<SelectListItem> AvailableTimeZones
        {
            get; set;
        }

        #endregion

        #region Authority program settings

        [Display(Name = "Max Days After Report Period End Date to Repudiate")]
        public string ReportRepudiatedDays
        {
            get; set;
        }

        [Editable(false)]
        public string ReportRepudiatedDaysDefault
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Mass Loading pounds Conversion Factor")]
        public string MassLoadingConversionFactorPounds
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Use < sign for Mass Loading Results")]
        public string MassLoadingResultToUseLessThanSign
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Use these decimal places for Loading calculations")]
        public string MassLoadingCalculationDecimalPlaces
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Number of Authority (Reg Program User) Licenses Available")]
        public string AuthorityUserLicenseTotalCount
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Number of Authority (Reg Program User) Licenses In Use")]
        public string AuthorityUserLicenseUsedCount
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Number of Industry (Reg Program) Licenses Available")]
        public string IndustryLicenseTotalCount
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Number of Industry (Reg Program) Licenses In Use")]
        public string IndustryLicenseUsedCount
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Number of Industry Users (Reg Program) per Industry")]
        public string UserPerIndustryMaxCount
        {
            get; set;
        }

        [Display(Name = "Name")]
        public string EmailContactInfoName
        {
            get; set;
        }

        [Editable(false)]
        public string EmailContactInfoNameDefault
        {
            get; set;
        }

        [Display(Name = "Phone")]
        public string EmailContactInfoPhone
        {
            get; set;
        }

        [Editable(false)]
        public string EmailContactInfoPhoneDefault
        {
            get; set;
        }

        [Display(Name = "Email")]
        [EmailAddress]
        public string EmailContactInfoEmailAddress
        {
            get; set;
        }

        #endregion        
    }
}