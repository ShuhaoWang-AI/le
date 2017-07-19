using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Email
{
    public class EmailEntry
    {
        public List<string> SendToEmails {get;set; }
        public EmailType  EmailType {get;set; }  

        public Dictionary<string,string> ContentReplacements {get;set; } 

        public int? ReceipientOrgulatoryProgramId {get;set; }
        public int? ReceipientOrganizationId {get;set; }
        public int? ReceipientRegulatorOrganizationId {get;set; }
    }
}
