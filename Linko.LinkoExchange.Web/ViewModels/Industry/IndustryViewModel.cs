using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Web.ViewModels.Industry
{
    public class IndustryViewModel
    {
        #region public properties

        [ScaffoldColumn(scaffold:false)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Industry Number")]
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

        #endregion
    }
}