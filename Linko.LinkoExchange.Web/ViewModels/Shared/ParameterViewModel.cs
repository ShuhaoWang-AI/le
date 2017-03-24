using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Web.ViewModels.Shared
{
    public class ParameterViewModel
    {
        [ScaffoldColumn(scaffold: false)]
        public int Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        [DataType(dataType: DataType.MultilineText)]
        public string Description { get; set; }

        public int DefaultUnitId { get; set; }

        [Display(Name = "Unit")]
        public string DefaultUnitName { get; set; }

        public bool IsRemoved { get; set; }
    }
}