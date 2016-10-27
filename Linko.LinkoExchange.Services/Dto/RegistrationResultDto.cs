using Linko.LinkoExchange.Core.Enum;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{

    public class RegistrationResultDto
    {
        public IEnumerable<string> Errors { get; set; }
        public RegistrationResult Result { get; set; }
    }
}
