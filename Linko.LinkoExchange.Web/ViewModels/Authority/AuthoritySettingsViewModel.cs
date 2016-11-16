using System.ComponentModel.DataAnnotations;

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
        public int TimeZone
        {
            get; set;
        }

        #endregion

        #region Authority program settings
        
        [Display(Name = "Max Days After Report Period End Date to Repudiate")]
        public int ReportRepudiatedDays
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
        public int MassLoadingConversionFactorPounds
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Use < sign for Mass Loading Results")]
        public int MassLoadingResultToUseLessThanSign
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Use these decimal places for Loading calculations")]
        public int MassLoadingCalculationDecimalPlaces
        {
            get; set;
        }
        
        [Display(Name = "Name")]
        public int EmailContactInfoName
        {
            get; set;
        }
        
        [Display(Name = "Phone")]
        public int EmailContactInfoPhone
        {
            get; set;
        }

        [Display(Name = "Email")]
        public int EmailContactInfoEmailAddress
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Number of Authority (Reg Program user) licenses Available")]
        public int AuthorityUserLicenseTotalCount
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Number of Authority (Reg Program user) licenses In Use")]
        public int AuthorityUserLicenseUsedCount
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Number of Industry (Reg Program) licenses Available")]
        public int IndustryLicenseTotalCount
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Number of Industry (Reg Program) licenses In Use")]
        public int IndustryLicenseUsedCount
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Number of Industry Users (Reg Program) per Industry")]
        public int UserPerIndustryMaxCount
        {
            get; set;
        }

        #endregion        
    }
}