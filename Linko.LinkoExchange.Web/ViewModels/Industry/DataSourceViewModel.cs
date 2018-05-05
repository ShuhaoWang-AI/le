using System;
using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Web.ViewModels.Industry
{
    public class DataSourceViewModel
    {
        #region public properties

        [ScaffoldColumn(scaffold:false)]
        [Display(Name = "Id")]
        public int? Id { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Editable(allowEdit:false)]
        [Display(Name = "Description")]
        [DataType(dataType: DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "Created/Modified Date")]
        public DateTime LastModificationDateTimeLocal { get; set; }

        [Display(Name = "Last Modified By")]
        public string LastModifierUserName { get; set; }
        #endregion
    }
}