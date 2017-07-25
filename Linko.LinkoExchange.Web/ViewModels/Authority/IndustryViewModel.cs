using System;
using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Web.ViewModels.Authority
{
    public class IndustryViewModel
    {
        [ScaffoldColumn(false)]
        [Display(Name = "Id")]
        public int Id
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Industry No.")]
        public int IndustryNo
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Industry No.")]
        public string ReferenceNumber { get; set; }

        [Editable(false)]
        [Display(Name = "Industry Name")]
        public string IndustryName
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

        private string address;
        [Editable(false)]
        [Display(Name = "Address")]
        public string Address
        {
            get
            {
                if (string.IsNullOrEmpty(address))
                {
                    string formattedAddress = string.Format(format: "{0} {1}, {2}, {3} {4}", args: new object[] { AddressLine1, AddressLine2, CityName, State, ZipCode });
                    formattedAddress = formattedAddress.Replace(" , , ", "");
                    formattedAddress = formattedAddress.Replace(", ,", ",");
                    formattedAddress = formattedAddress.Replace(" , ", ", ").Trim();
                    if (formattedAddress.Length > 0 && formattedAddress[0] == ',')
                    {
                        //trim leading comma
                        formattedAddress = formattedAddress.Substring(1);
                    }
                    if (formattedAddress.Length > 0 && formattedAddress[formattedAddress.Length - 1] == ',')
                    {
                        //trim trailing comma
                        formattedAddress = formattedAddress.Substring(0, formattedAddress.Length - 1);
                    }

                    return formattedAddress.Trim();
                }
                else
                {
                    return address;
                }
            }
            set {
                address = value;
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
        [Display(Name = "Classification")]
        public string Classification
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Enabled")]
        public bool IsEnabled
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Enabled")]
        public string IsEnabledText
        {
            get
            {
                return IsEnabled ? "Yes" : "No";
            }
        }

        [Editable(false)]
        [Display(Name = "Has Signatory")]
        public bool HasSignatory
        {
            get; set;
        }

        [Editable(false)]
        [Display(Name = "Has Signatory")]
        public string HasSignatoryText
        {
            get
            {
                return HasSignatory ? "Yes" : "No";
            }
        }

        [Editable(false)]
        [Display(Name = "Assigned To")]
        public string AssignedTo
        {
            get; set;
        }
                
        [Editable(false)]
        [Display(Name = "Last Submission")]
        public DateTime? LastSubmission
        {
            get; set;
        }

        [Editable(false)]
        public bool HasPermissionForEnableDisable
        {
            get; set;
        }
    }
}