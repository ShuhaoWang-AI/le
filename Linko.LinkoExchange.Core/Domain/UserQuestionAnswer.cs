﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    public class UserQuestionAnswer
    {
        public int UserQuestionAnswerId { get; set; }
        public string Content { get; set; }
        public int QuestionId { get; set; }
        public int UserProfileId { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime LastModificationDateTime { get; set; }
    }
}
