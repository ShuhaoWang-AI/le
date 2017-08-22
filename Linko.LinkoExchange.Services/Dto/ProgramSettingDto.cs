using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ProgramSettingDto
    {
        #region public properties

        public ICollection<SettingDto> Settings { get; set; }
        public int OrgRegProgId { get; set; }

        #endregion
    }
}