using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ProgramSettingDto
    {
        public IEnumerable<SettingDto> Settings { get; set; }
        public int ProgramId { get; set; }
    }
}