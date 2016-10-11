using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    public class UserPasswordHistory
    {
        [Key]
        public int UserPasswordHistoryId { get; set; }
        public string PasswordHash  { get; set; }
        public int UserProfileId { get; set; }
        public DateTime LastModificationDateTime { get; set; }
    }
}
