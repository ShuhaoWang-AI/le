using System.Collections.Generic; 

namespace Linko.LinkoExchange.Services.Dto
{
    public class OrganizationSettingDto
    {
        public ICollection<SettingDto> Settings { get; set; }
        public ICollection<ProgramSettingDto> ProgramSettings { get; set; }
        public int OrganizationId { get; set; }
    }
}
