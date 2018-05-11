using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Web.ViewModels.Shared
{
    public class DropdownOptionViewModel
    {
        #region public properties

        [ScaffoldColumn(scaffold:false)]
        public int? Id { get; set; }

        public string DisplayName { get; set; }

        #endregion
    }
}