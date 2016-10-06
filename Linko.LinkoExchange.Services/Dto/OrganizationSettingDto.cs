using System.Collections.Generic; 

namespace Linko.LinkoExchange.Services.Dto
{
    public class OrganizationSettingDto
    {
        public IEnumerable<SettingDto> Settings { get; set; }
        public IEnumerable<ProgramSettingDto> ProgramSettings { get; set; }
        public int OrganizationId { get; set; }
    }
}
