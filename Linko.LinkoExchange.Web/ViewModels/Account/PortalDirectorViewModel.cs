using System.Collections.Generic;
using System.Web.Mvc;

namespace Linko.LinkoExchange.Web.ViewModels.Account
{
    public class PortalDirectorViewModel
    {
        public IEnumerable<SelectListItem> Authorities
        {
            get; set;
        }
        public IEnumerable<SelectListItem> Industries
        {
            get; set;
        }
    }
}