using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Linko.LinkoExchange.Web.ViewModels.Shared
{
    public class SelectedParentChildCombination
    {
        public int Id { get; set; }
        public List<ChildElement> ChildElements { get; set; }
    }
    public partial class ChildElement
    {
        public int Id { get; set; }
    }
}