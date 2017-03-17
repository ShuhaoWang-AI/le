using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Linko.LinkoExchange.Web.ViewModels.Shared
{
    public class ParameterGroupViewModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }
        public string LastModifierUserName { get; set; }
        public virtual ICollection<ParameterViewModel> Parameters { get; set; }
    }
}