using System;
using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Web.ViewModels.Authority
{
    public class IndustryViewModel
    {
        #region fields

        private string address;

        #endregion

        #region public properties

        [ScaffoldColumn(scaffold:false)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Industry No.")]
        public int IndustryNo { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Industry No.")]
        public string ReferenceNumber { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Industry Name")]
        public string IndustryName { get; set; }

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
        public string Address
        {
            get
            {
                if (string.IsNullOrEmpty(value:address))
                {
                    var formattedAddress = string.Format(format:"{0} {1}, {2}, {3} {4}", args:new object[] {AddressLine1, AddressLine2, CityName, State, ZipCode});
                    formattedAddress = formattedAddress.Replace(oldValue:" , , ", newValue:"");
                    formattedAddress = formattedAddress.Replace(oldValue:", ,", newValue:",");
                    formattedAddress = formattedAddress.Replace(oldValue:" , ", newValue:", ").Trim();
                    if (formattedAddress.Length > 0 && formattedAddress[index:0] == ',')
                    {
                        //trim leading comma
                        formattedAddress = formattedAddress.Substring(startIndex:1);
                    }
                    if (formattedAddress.Length > 0 && formattedAddress[index:formattedAddress.Length - 1] == ',')
                    {
                        //trim trailing comma
                        formattedAddress = formattedAddress.Substring(startIndex:0, length:formattedAddress.Length - 1);
                    }

                    return formattedAddress.Trim();
                }
                else
                {
                    return address;
                }
            }
            set { address = value; }
        }

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
        [Display(Name = "Classification")]
        public string Classification { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Enabled")]
        public bool IsEnabled { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Enabled")]
        public string IsEnabledText => IsEnabled ? "Yes" : "No";

        [Editable(allowEdit:false)]
        [Display(Name = "Has Signatory")]
        public bool HasSignatory { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Has Signatory")]
        public string HasSignatoryText => HasSignatory ? "Yes" : "No";

        [Editable(allowEdit:false)]
        [Display(Name = "Assigned To")]
        public string AssignedTo { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Last Submission")]
        public DateTime? LastSubmission { get; set; }

        [Editable(allowEdit:false)]
        public bool HasPermissionForEnableDisable { get; set; }

        #endregion
    }
}