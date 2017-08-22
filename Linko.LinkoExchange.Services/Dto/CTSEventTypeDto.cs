using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class CtsEventTypeDto
    {
        #region public properties

        public int CtsEventTypeId { get; set; }

        public string Name { get; set; }
        public string CtsEventCategoryName { get; set; }
        public string Description { get; set; }
        public DateTime LastModificationDateTimeLocal { get; set; }
        public string LastModifierFullName { get; set; }

        #endregion
    }
}