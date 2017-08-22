using System.Collections.Generic;

namespace Linko.LinkoExchange.Web.ViewModels.Shared
{
    public class SelectedParentChildCombination
    {
        #region public properties

        public int Id { get; set; }
        public List<ChildElement> ChildElements { get; set; }

        #endregion
    }

    public class ChildElement
    {
        #region public properties

        public int Id { get; set; }

        #endregion
    }
}