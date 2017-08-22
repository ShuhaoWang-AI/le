using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class RepudiationReasonDto
    {
        #region public properties

        public int RepudiationReasonId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreationLocalDateTime { get; set; }
        public DateTime? LastModificationLocalDateTime { get; set; }

        #endregion
    }
}