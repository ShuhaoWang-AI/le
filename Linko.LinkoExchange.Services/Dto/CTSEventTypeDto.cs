
using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class CtsEventTypeDto
    {
        public int CtsEventTypeId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
        public DateTime LastModificationDateTimeLocal { get; set; }
        public string LastModifierFullName { get; set; }

    }
}