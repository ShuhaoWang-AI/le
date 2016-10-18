using System.Collections.Generic;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Dto
{
    public class InvitationServiceResultDto
    {
        public InvitationServiceResultDto()
        {
            Success = false;
            ErrorType = null;
        }

        public InvitationDto InvitationDto { get; set; }
        public bool Success { get; set; }  
        public InvitationError? ErrorType { get; set; }
        public IEnumerable<string> Errors { get; set; }
    } 
}
