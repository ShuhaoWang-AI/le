using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ParameterGroupDto
    {
        #region public properties

        public int? ParameterGroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastModificationDateTimeLocal { get; set; }
        public string LastModifierFullName { get; set; }
        public virtual ICollection<ParameterDto> Parameters { get; set; }

        #endregion
    }
}