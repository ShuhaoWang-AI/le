using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{
    public class AuthenticationResultDto
    {
        public bool Success { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}