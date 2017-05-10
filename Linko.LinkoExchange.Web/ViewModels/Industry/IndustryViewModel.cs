using System;
using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Web.ViewModels.Industry
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
        [Display(Name = "Industry Number")]
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
    }
}