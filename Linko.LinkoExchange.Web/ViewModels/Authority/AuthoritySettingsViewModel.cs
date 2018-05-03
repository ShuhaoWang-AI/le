using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Resources;

namespace Linko.LinkoExchange.Web.ViewModels.Authority
{
    [Validator(validatorType:typeof(AuthoritySettingsViewModelValidator))]
    public class AuthoritySettingsViewModel
    {
        #region Authority information

        [ScaffoldColumn(scaffold:false)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Exchange Authority Id")]
        public int ExchangeAuthorityId { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Authority Name")]
        public string AuthorityName { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "NPDES")]
        public string Npdes { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Signer")]
        public string Signer { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Address line 1")]
        public string AddressLine1 { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Address line 2")]
        public string AddressLine2 { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "City")]
        public string CityName { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "State")]
        public string State { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Address")]
        public string Address => string.Format(format:"{0} {1}, {2}, {3} {4}", args:new object[] {AddressLine1, AddressLine2, CityName, State, ZipCode});

        [Editable(allowEdit:false)]
        [Display(Name = "Phone Number")]
        [Phone]
        public string PhoneNumber { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Ext")]
        public string PhoneExt { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Fax Number")]
        [Phone]
        public string FaxNumber { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Website URL")]
        public string WebsiteUrl { get; set; }

        [Editable(allowEdit:false)]
        public bool HasPermissionForUpdate { get; set; }

        #endregion

        #region Authority settings

        [Display(Name = "Number of Allowed Failed Password Attempts")]
        public string FailedPasswordAttemptMaxCount { get; set; }

        [Editable(allowEdit:false)]
        public string FailedPasswordAttemptMaxCountDefault { get; set; }

        [Display(Name = "Number of Allowed Failed KBQ Attempts")]
        public string FailedKbqAttemptMaxCount { get; set; }

        [Editable(allowEdit:false)]
        public string FailedKbqAttemptMaxCountDefault { get; set; }

        [Display(Name = "Invitation Expires This Many Hours After Sending")]
        public string InvitationExpiredHours { get; set; }

        [Editable(allowEdit:false)]
        public string InvitationExpiredHoursDefault { get; set; }

        [Display(Name = "Number of Days Before Requiring a Password Change")]
        public string PasswordChangeRequiredDays { get; set; }

        [Editable(allowEdit:false)]
        public string PasswordChangeRequiredDaysDefault { get; set; }

        [Display(Name = "Number of Passwords Used in Password History")]
        public string PasswordHistoryMaxCount { get; set; }

        [Editable(allowEdit:false)]
        public string PasswordHistoryMaxCountDefault { get; set; }

        [Display(Name = "Time Zone")]
        public string TimeZone { get; set; }

        public IList<SelectListItem> AvailableTimeZones { get; set; }

        #endregion

        #region Authority program settings

        [Display(Name = "Max Days After Report Period End Date to Repudiate")]
        public string ReportRepudiatedDays { get; set; }

        [Editable(allowEdit:false)]
        public string ReportRepudiatedDaysDefault { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Compliance Determination Date")]
        public ComplianceDeterminationDate ComplianceDeterminationDate { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Mass Loading pounds Conversion Factor")]
        public string MassLoadingConversionFactorPounds { get; set; }

        [Display(Name = "Select the Result Qualifiers the Industry Can Use")]
        public string ResultQualifierValidValues { get; set; }

        public IList<SelectListItem> AvailableResultQualifierValidValues { get; set; }

        [Display(Name = "Select the Sample Flow Units the Industry Can Use")]
        public string FlowUnitValidValues { get; set; }

        public IList<SelectListItem> AvailableFlowUnitValidValues { get; set; }

        [Display(Name = "Create Sample Name Using")]
        public string SampleNameCreationRule { get; set; }

        public IList<SelectListItem> AvailableSampleNameCreationRule
        {
            get
            {
                var selectedValues = SampleNameCreationRule?.Split(',').ToList() ?? new List<string> {""};
                var options = Enum.GetValues(enumType:typeof(SampleNameCreationRuleOption))
                                  .Cast<SampleNameCreationRuleOption>()
                                  .Select(
                                          x => new SelectListItem
                                               {
                                                   Text = Label.ResourceManager.GetString(name:x.ToString()),
                                                   Value = x.ToString(),
                                                   Selected = selectedValues.Contains(item:x.ToString())
                                               }
                                         ).ToList();
                return options;
            }
        }

        [Editable(allowEdit:false)]
        [Display(Name = "Use < sign for Mass Loading Results")]
        public string MassLoadingResultToUseLessThanSign { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Use these decimal places for Loading calculations")]
        public string MassLoadingCalculationDecimalPlaces { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Number of Authority Licenses Available")]
        public string AuthorityUserLicenseTotalCount { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Number of Authority Licenses In Use")]
        public string AuthorityUserLicenseUsedCount { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Number of Industry Licenses Available")]
        public string IndustryLicenseTotalCount { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Number of Industry Licenses In Use")]
        public string IndustryLicenseUsedCount { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Number of Industry Users per Industry")]
        public string UserPerIndustryMaxCount { get; set; }

        [Display(Name = "Contact Information Name on Emails")]
        public string EmailContactInfoName { get; set; }

        [Editable(allowEdit:false)]
        public string EmailContactInfoNameDefault { get; set; }

        [Display(Name = "Contact Information Phone on Emails")]
        [Phone]
        public string EmailContactInfoPhone { get; set; }

        [Editable(allowEdit:false)]
        [Phone]
        public string EmailContactInfoPhoneDefault { get; set; }

        [Display(Name = "Contact Information Email Address on Emails")]
        [EmailAddress]
        public string EmailContactInfoEmailAddress { get; set; }

        [Display(Name = "Attachment Type for Industry File Upload")]
        public string ReportElementTypeIdForIndustryFileUpload { get; set; }
        public IList<SelectListItem> AvailableReportElementTypes { get; set; }

        #endregion
    }

    public class AuthoritySettingsViewModelValidator : AbstractValidator<AuthoritySettingsViewModel>
    {
        #region constructors and destructor

        public AuthoritySettingsViewModelValidator()
        {
            //Sample flow unit
            When(x => x.AvailableFlowUnitValidValues != null, () =>
                                                              {
                                                                  RuleFor(x => x.AvailableFlowUnitValidValues.Count(c => c.Selected))
                                                                      .GreaterThan(valueToCompare:0)
                                                                      .WithMessage(errorMessage:"At least one sample flow unit must be selected.");
                                                              });

            RuleFor(x => x.ReportElementTypeIdForIndustryFileUpload).NotEmpty().NotEqual(toCompare:"0").WithMessage(errorMessage:"{PropertyName} is required.");
        }

        #endregion
    }
}